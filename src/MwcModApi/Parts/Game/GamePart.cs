using System;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using MSCLoader;
using MwcModApi.Caching;
using MwcModApi.Parts.EventSystem;
using MwcModApi.Saving;
using MwcModApi.Tools;
using UnityEngine;

namespace MwcModApi.Parts.Game
{
	/// <summary>
	/// (NOTE this class is considered "BETA" with not a lot of tests for different parts)
	/// A wrapper for parts made/added by the game itself
	/// In the future, this may become a replacement for the 'OldPart' class used for the 'ReplacementPart' as a more generic Wrapper
	/// </summary>
	public class GamePart : BasicPart, SupportsPartEvents
	{
		/// <summary>
		/// ID of the GamePart used for saving, the mainFsmPartName string from constructor is used to define this id.
		/// </summary>
		public readonly string id;

		public readonly string partName;

		protected GameObject currentPhysicalPart = null;
		protected string physicalPartId = "";

		public GamePartSave saveData => new GamePartSave(installedOnCar, position, Quaternion.Euler(rotation));

		/// <summary>
		/// Stores all events that a developer may have added to this GamePart object
		/// </summary>
		protected Dictionary<PartEvent.Time, Dictionary<PartEvent.Type, PartEventListenerCollection>> events =
			new Dictionary<PartEvent.Time, Dictionary<PartEvent.Type, PartEventListenerCollection>>();

		/// <summary>
		/// Flag used to avoid calling the pre bolted event multiple times
		/// </summary>
		protected bool alreadyCalledPreBolted;

		/// <summary>
		/// Flag used to avoid calling the post bolted event multiple times
		/// </summary>
		protected bool alreadyCalledPostBolted;

		/// <summary>
		/// Flag used to avoid calling the pre unbolted event multiple times
		/// </summary>
		protected bool alreadyCalledPreUnbolted = true;

		/// <summary>
		/// Flag used to avoid calling the post unbolted event multiple times
		/// </summary>
		protected bool alreadyCalledPostUnbolted = true;

		/// <summary>
		/// Creates a new GamePart wrapper object
		/// </summary>
		/// <param name="installPointFsmName">The main GameObject name (capital letter name) Ex.: "VINP_Carburettor"</param>
		/// <param name="partName">THe name of the actual part Ex.: "4 Barrell Racing Carb(VINXX)"</param>
		/// <param name="physicalPartId">The ID of the physical part, some parts may have the same "partName" but are different physical parts, this ID can be found in the Data PlayMakerFSM component under FsmString</param>
		public GamePart(string installPointFsmName, string partName, string physicalPartId = "")
		{
			InitEventStorage();
			id = installPointFsmName + "-" + partName;
			this.partName = partName;
			this.physicalPartId = physicalPartId;

			installPointFsmGameObject = Cache.Find(installPointFsmName);
			if (!installPointFsmGameObject) {
				throw new Exception($"Unable to find main fsm part GameObject using '{installPointFsmName}'");
			}

			dataFsm = installPointFsmGameObject.FindFsm("Data");
			if (!dataFsm) {
				throw new Exception(
					$"Unable to find data fsm on GameObject with name '{installPointFsmGameObject.name}'"
				);
			}

			nearState = dataFsm.FindState("Near");
			if (nearState == null) {
				throw new Exception(
					$"Unable to find 'Near' state on GameObject with name '{installPointFsmGameObject.name}'"
				);
			}

			currentPhysicalPart = GetCurrentPhysicalPart();

			boltedState = dataFsm.FsmVariables.FindFsmBool("Bolted");
			damagedState = dataFsm.FsmVariables.FindFsmBool("Damaged") ?? new FsmBool("Damaged");
			installedState = dataFsm.FsmVariables.FindFsmBool("Installed") ?? new FsmBool("Installed");
			purchasedState = dataFsm.FsmVariables.FindFsmBool("Purchased") ?? new FsmBool("Purchased");

			tightness = dataFsm.FsmVariables.FindFsmFloat("Tightness");
			if (tightness == null) {
				throw new Exception($"Unable to find tightness on part '{installPointFsmGameObject.name}'");
			}

			dataFsm.FindState("Installed").AddActionAsFirst(
				() => { GetEventListeners(PartEvent.Time.Pre, PartEvent.Type.Install).InvokeAll(); },
				"MwcModApi-Install-Pre"
			);

			dataFsm.FindState("Installed").AddActionAsLast(
				() =>
				{
					currentPhysicalPart = GetCurrentPhysicalPart();

					SetupBoltedStateDetection(currentPhysicalPart);

					GetEventListeners(PartEvent.Time.Post, PartEvent.Type.Install).InvokeAll();
					if (installedOnCar) {
						GetEventListeners(PartEvent.Time.Post, PartEvent.Type.InstallOnCar).InvokeAll();
					}
				}, "MwcModApi-Install-Post"
			);

			dataFsm.FindState("Remove part").AddActionAsFirst(
				() => { GetEventListeners(PartEvent.Time.Pre, PartEvent.Type.Uninstall).InvokeAll(); },
				"MwcModApi-Uninstall-Pre"
			);
			dataFsm.FindState("Remove part").AddActionAsLast(
				() =>
				{
					GetEventListeners(PartEvent.Time.Post, PartEvent.Type.Uninstall).InvokeAll();
					if (!installedOnCar) {
						//Check probably not needed, likely already not on car because part can't be connected to something else after being uninstalled
						GetEventListeners(PartEvent.Time.Post, PartEvent.Type.UninstallFromCar).InvokeAll();
					}

					RemoveBoltedStateDetection(currentPhysicalPart);
					currentPhysicalPart = null;
				}, "MwcModApi-Uninstall-Post"
			);
		}

		/// <summary>
		/// Usable when wanting to extend from GamePart and implement everything yourself.
		/// An Example for the usability of this is the Class "CarGamePart" which is a wrapper to make the car
		/// (which has no part logic from the game) to allow using as a parent for "Part" objects
		/// </summary>
		protected GamePart()
		{
		}

		protected GameObject GetCurrentPhysicalPart()
		{
			GameObject part = installPointFsmGameObject.FindChild(partName);
			if (part == null) {
				return null;
			}

			if (physicalPartId == "") {
				return part;
			}

			PlayMakerFSM fsm = part.FindFsm("Data");
			if (fsm == null) {
				return null;
			}

			FsmString id = fsm.FsmVariables.GetFsmString("ID");
			if (id == null || id.Value != physicalPartId) {
				return null;
			}

			return part;
		}

		protected void RemoveBoltedStateDetection(GameObject currentPhysicalPart)
		{
			PlayMakerFSM activePartDataFsm = currentPhysicalPart.FindFsm("Data");

			FsmState boltedState = activePartDataFsm.FindState("Bolted");
			FsmState unboltedState = activePartDataFsm.FindState("Unbolted");

			boltedState.RemoveActionByName("MwcModApi-Bolted-Pre");
			boltedState.RemoveActionByName("MwcModApi-Bolted-Post");
			boltedState.RemoveActionByName("MwcModApi-Unbolted-Pre");
			boltedState.RemoveActionByName("MwcModApi-Unbolted-Post");
		}

		/// <summary>
		/// Setups the simple bolted state detection requiring just the "Bolted" state
		/// of the part to change state to true/false for events to trigger
		/// </summary>
		protected void SetupBoltedStateDetection(GameObject currentPhysicalPart)
		{
			PlayMakerFSM activePartDataFsm = currentPhysicalPart.FindFsm("Data");

			FsmState boltedState = activePartDataFsm.FindState("Bolted");
			FsmState unboltedState = activePartDataFsm.FindState("Unbolted");

			unboltedState.AddActionAsFirst(
				() =>
				{
					alreadyCalledPreBolted = false;

					if (alreadyCalledPreUnbolted) {
						return;
					}

					GetEventListeners(PartEvent.Time.Pre, PartEvent.Type.Unbolted).InvokeAll();
					if (installedOnCar) {
						GetEventListeners(PartEvent.Time.Pre, PartEvent.Type.UnboltedOnCar).InvokeAll();
					}
				}, "MwcModApi-Unbolted-Pre"
			);

			unboltedState.AddActionAsLast(
				() =>
				{
					alreadyCalledPostBolted = false;

					if (alreadyCalledPostUnbolted) {
						return;
					}

					GetEventListeners(PartEvent.Time.Post, PartEvent.Type.Unbolted).InvokeAll();
					if (installedOnCar) {
						GetEventListeners(PartEvent.Time.Post, PartEvent.Type.UnboltedOnCar).InvokeAll();
					}
				}, "MwcModApi-Unbolted-Post"
			);

			boltedState.AddActionAsFirst(
				() =>
				{
					alreadyCalledPreUnbolted = false;

					if (alreadyCalledPreBolted) {
						return;
					}

					GetEventListeners(PartEvent.Time.Pre, PartEvent.Type.Bolted).InvokeAll();
					if (installedOnCar) {
						GetEventListeners(PartEvent.Time.Pre, PartEvent.Type.BoltedOnCar).InvokeAll();
					}
				}, "MwcModApi-Bolted-Pre"
			);

			boltedState.AddActionAsLast(
				() =>
				{
					alreadyCalledPostUnbolted = false;

					if (alreadyCalledPostBolted) {
						return;
					}

					GetEventListeners(PartEvent.Time.Post, PartEvent.Type.Bolted).InvokeAll();
					if (installedOnCar) {
						GetEventListeners(PartEvent.Time.Post, PartEvent.Type.BoltedOnCar).InvokeAll();
					}
				}, "MwcModApi-Bolted-Post"
			);
		}

		/// <summary>
		/// Initializes the event dictionary
		/// </summary>
		protected void InitEventStorage()
		{
			foreach (PartEvent.Time eventTime in Enum.GetValues(typeof(PartEvent.Time))) {
				Dictionary<PartEvent.Type, PartEventListenerCollection> TypeDict =
					new Dictionary<PartEvent.Type, PartEventListenerCollection>();

				foreach (PartEvent.Type Type in Enum.GetValues(typeof(PartEvent.Type))) {
					TypeDict.Add(Type, new PartEventListenerCollection());
				}

				events.Add(eventTime, TypeDict);
			}
		}

		/// <summary>
		/// Block installation of the part by disabling the trigger object
		/// </summary>
		public override bool installBlocked
		{
			get { return nearState.Actions.Any(action => !action.Enabled); }
			set
			{
				foreach (var action in nearState.Actions) {
					action.Enabled = !value;
				}
			}
		}

		/// <summary>
		/// The parts tightness (sum of all screw tightness (8 x screw count = all bolted))
		/// </summary>
		public FsmFloat tightness { get; protected set; }

		/// <summary>
		/// Returns the max tightness of the part
		/// (Sum of all screws found x 8)
		/// (only set when using advanced bolted state detection)
		/// 
		/// </summary>
		public float maxTightness { get; protected set; }

		/// <summary>
		/// The main Fsm GameObject
		/// </summary>
		public GameObject installPointFsmGameObject { get; protected set; }

		/// <summary>
		/// The data FSM object of the part
		/// </summary>
		public PlayMakerFSM dataFsm { get; protected set; }

		/// <summary>
		/// Returns if the part is bought
		/// (defaults to false)
		/// </summary>
		public FsmBool purchasedState { get; protected set; }

		/// <summary>
		/// Returns if the part is installed
		/// (defaults to false)
		/// </summary>
		public FsmBool installedState { get; protected set; }

		/// <summary>
		/// Returns if the part is damaged
		/// (defaults to false)
		/// </summary>
		public FsmBool damagedState { get; protected set; }

		/// <summary>
		/// Returns if the part is bought
		/// (defaults to false)
		/// </summary>
		public FsmBool boltedState { get; protected set; }

		/// <summary>
		/// The root GameObject which acts as the parent & install point of the actual physical object
		/// </summary>
		public override GameObject gameObject
		{
			get => installPointFsmGameObject;
			protected set => installPointFsmGameObject = value;
		}

		FsmState nearState { get; }

		/// <inheritdoc />
		public override bool bought
		{
			get => purchasedState.Value;
			set => purchasedState.Value = value;
		}

		/// <inheritdoc />
		public override Vector3 position
		{
			get => gameObject.transform.position;
			set
			{
				if (!installed) {
					gameObject.transform.position = value;
				}
			}
		}

		/// <inheritdoc />
		public override Vector3 rotation
		{
			get => gameObject.transform.rotation.eulerAngles;
			set
			{
				if (!installed) {
					gameObject.transform.rotation = Quaternion.Euler(value);
				}
			}
		}

		/// <summary>
		/// Returns if the game part is installed
		/// </summary>
		public override bool installed =>
			installedState.Value && currentPhysicalPart != null && currentPhysicalPart.name == partName;

		/// <summary>
		/// Returns if the game part is bolted
		/// </summary>
		public override bool bolted =>
			(boltedState?.Value ?? false) && currentPhysicalPart != null &&
			currentPhysicalPart.name == partName;

		/// <inheritdoc />
		public override bool hasBolts => dataFsm.FsmVariables.FindFsmBool("Bolted") != null;

		/// <inheritdoc />
		public override bool installedOnCar => currentPhysicalPart.transform.root == CarH.car.transform;

		/// <inheritdoc />
		public override bool active
		{
			get => gameObject.activeSelf;
			set => gameObject.SetActive(value);
		}

		/// <inheritdoc />
		public override string name => gameObject.name;

		/// <inheritdoc />
		public override bool isLookingAt =>
		(
			Camera.main != null
			&& Physics.Raycast(
				Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 1f,
				1 << gameObject.layer
			)
			&& hit.collider.gameObject.name == partName
		);

		/// <inheritdoc />
		public override bool isHolding => partName == Player.currentObjectInHand.name;

		/// <summary>
		/// Sends the REMOVE event to the Part
		/// </summary>
		public override void Uninstall()
		{
			dataFsm.SendEvent("REMOVE");
		}

		public override void ResetToDefault(bool uninstall = false)
		{
			if (uninstall && installed) {
				Uninstall();
			}

			position = defaultPosition;
			rotation = defaultRotation;
		}

		/// <summary>
		/// Gets called by the advanced bolted state detection when Unscrewing a bolt
		/// </summary>
		/// <param name="eventTime"></param>
		protected void OnUnscrew(PartEvent.Time eventTime)
		{
			alreadyCalledPreBolted = false;
			alreadyCalledPostBolted = false;

			switch (eventTime) {
				case PartEvent.Time.Pre:
					if (alreadyCalledPreUnbolted) {
						return;
					}

					alreadyCalledPreUnbolted = true;
					GetEventListeners(PartEvent.Time.Pre, PartEvent.Type.Unbolted).InvokeAll();
					if (installedOnCar) {
						GetEventListeners(PartEvent.Time.Pre, PartEvent.Type.UnboltedOnCar).InvokeAll();
					}

					break;
				case PartEvent.Time.Post:
					if (alreadyCalledPostUnbolted) {
						return;
					}

					alreadyCalledPostUnbolted = true;
					GetEventListeners(PartEvent.Time.Post, PartEvent.Type.Unbolted).InvokeAll();
					if (installedOnCar) {
						GetEventListeners(PartEvent.Time.Post, PartEvent.Type.UnboltedOnCar).InvokeAll();
					}

					break;
			}
		}

		/// <summary>
		/// Gets called by the advanced bolted state detection when a bolt reaches the state "8" (tight)
		/// </summary>
		/// <param name="eventTime"></param>
		protected void OnTight(PartEvent.Time eventTime)
		{
			if (tightness.Value < maxTightness) {
				return; //Wait for all screws to be tight
			}

			alreadyCalledPreUnbolted = false;
			alreadyCalledPostUnbolted = false;

			switch (eventTime) {
				case PartEvent.Time.Pre:
					if (alreadyCalledPreBolted) {
						return;
					}

					alreadyCalledPreBolted = true;
					GetEventListeners(PartEvent.Time.Pre, PartEvent.Type.Bolted).InvokeAll();
					if (installedOnCar) {
						GetEventListeners(PartEvent.Time.Pre, PartEvent.Type.BoltedOnCar).InvokeAll();
					}

					break;
				case PartEvent.Time.Post:
					if (alreadyCalledPostBolted) {
						return;
					}

					alreadyCalledPostBolted = true;
					GetEventListeners(PartEvent.Time.Post, PartEvent.Type.Bolted).InvokeAll();
					if (installedOnCar) {
						GetEventListeners(PartEvent.Time.Post, PartEvent.Type.BoltedOnCar).InvokeAll();
					}

					break;
			}
		}

		/// <summary>
		/// Not implemented for the 
		/// </summary>
		/// <param name="eventTime"></param>
		/// <param name="Type"></param>
		/// <returns></returns>
		public PartEventListenerCollection GetEventListeners(PartEvent.Time eventTime, PartEvent.Type Type)
		{
			return events[eventTime][Type];
		}

		/// <inheritdoc />
		public PartEventListener AddEventListener(
			PartEvent.Time eventTime,
			PartEvent.Type Type,
			Action action,
			bool invokeActionIfConditionMet = true
		)
		{
			PartEventListener partEventListener = new PartEventListener(eventTime, Type, action);

			if (
				eventTime == PartEvent.Time.Pre
				&& (Type == PartEvent.Type.InstallOnCar || Type == PartEvent.Type.UninstallFromCar)
			) {
				throw new Exception($"Event {Type} can't be detected at '{eventTime}'. Unsupported!");
			}

			events[eventTime][Type].Add(partEventListener);

			if (invokeActionIfConditionMet && eventTime == PartEvent.Time.Post) {
				switch (Type) {
					//ToDo: check if invoking just the newly added action is enough of if all have to be invoked
					case PartEvent.Type.Install:
						if (installed) {
							action.Invoke();
						}

						break;
					case PartEvent.Type.Uninstall:
						if (!installed) {
							action.Invoke();
						}

						break;
					case PartEvent.Type.Bolted:
						if (bolted) {
							//ToDo: bolted state should only be true if maxTightness is also reached
							action.Invoke();
						}

						break;
					case PartEvent.Type.Unbolted:
						if (!bolted) {
							//ToDo: bolted state should only be true if maxTightness is also reached
							action.Invoke();
						}

						break;
					case PartEvent.Type.InstallOnCar:
						if (installedOnCar) {
							action.Invoke();
						}

						break;
					case PartEvent.Type.UninstallFromCar:
						if (!installedOnCar) {
							action.Invoke();
						}

						break;
					case PartEvent.Type.BoltedOnCar:
						if (bolted && installedOnCar) {
							action.Invoke();
						}

						break;
					case PartEvent.Type.UnboltedOnCar:
						if (!bolted && installedOnCar) {
							action.Invoke();
						}

						break;
				}
			}

			return partEventListener;
		}

		/// <inheritdoc />
		public bool RemoveEventListener(PartEventListener partEventListener)
		{
			var collection = GetEventListeners(partEventListener.eventTime, partEventListener.type);
			return collection.Contains(partEventListener) && collection.Remove(partEventListener);
		}

		/// <summary>
		/// When this part installs, the "partsToBlock" parts will be blocked from being installed (installBlocked = true)
		/// When this part uninstalls (opposite of "Type") the "partsToBlock" parts will be unblocked from being installed (installBlocked = false)
		/// </summary>
		/// <param name="Type">The event of this part after which installation of the "partsToBlock" will be blocked/unblocked</param>
		/// <param name="partsToBlock">The parts to block when the "Type" is called on this part</param>
		public void BlockOtherPartInstallOnEvent(PartEvent.Type Type, IEnumerable<BasicPart> partsToBlock)
		{
			AddEventListener(
				PartEvent.Time.Post, Type, () =>
				{
					foreach (BasicPart partToBlock in partsToBlock) {
						partToBlock.installBlocked = true;
					}
				}
			);
			AddEventListener(
				PartEvent.Time.Post, PartEvent.GetOppositeEvent(Type), () =>
				{
					foreach (BasicPart partToBlock in partsToBlock) {
						partToBlock.installBlocked = false;
					}
				}
			);
		}

		/// <summary>
		/// When this part installs, the "partToBlock" part will be blocked from being installed (installBlocked = true)
		/// When this part uninstalls (opposite of "Type") the "partToBlock" part will be unblocked from being installed (installBlocked = false)
		/// </summary>
		/// <param name="Type">The event of this part after which installation of the "partToBlock" will be blocked/unblocked</param>
		/// <param name="partToBlock">The part to block when the "Type" is called on this part</param>
		public void BlockOtherPartInstallOnEvent(PartEvent.Type Type, BasicPart partToBlock)
		{
			AddEventListener(PartEvent.Time.Post, Type, () =>
			{
				partToBlock.installBlocked = true;
			});
			AddEventListener(
				PartEvent.Time.Post, PartEvent.GetOppositeEvent(Type),
				() =>
				{
					partToBlock.installBlocked = false;
				}
			);
		}
	}
}