using System;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using MwcModApi.Caching;
using MwcModApi.Parts.EventSystem;
using UnityEngine;

namespace MwcModApi.Parts.Game
{
	/// <summary>
	/// A wrapper class to allow using the car GameObject as if it was able to use GamePart logic
	/// </summary>
	public class CarGamePart : GamePart
	{
		/// <summary>
		/// Instance of this class
		/// </summary>
		protected static CarGamePart instance;

		private CarGamePart()
		{
			tightness = new FsmFloat
			{
				Value = 8
			};

			purchasedState = new FsmBool
			{
				Value = true
			};

			purchasedState = new FsmBool
			{
				Value = true
			};
			damagedState = new FsmBool
			{
				Value = false
			};

			boltedState = new FsmBool
			{
				Value = true
			};

			gameObject = CarH.car;
		}

		public new bool installBlocked => gameObject.activeSelf;

		public new float maxTightness => 8;

		public new Vector3 position => gameObject.transform.position;

		public new Vector3 rotation => gameObject.transform.rotation.eulerAngles;

		public override bool installedOnCar => true;

		/// <summary>
		/// Returns the instance to the CarGamePart
		/// The CarGamePart can only ever exist once, so Singleton pattern is used to avoid multiple objects existing
		/// </summary>
		/// <returns></returns>
		public static CarGamePart GetInstance()
		{
			if (instance != null) {
				return instance;
			}

			instance = new CarGamePart();
			return instance;
		}

		/// <summary>
		/// Cleanup static fields
		/// </summary>
		public static void LoadCleanup()
		{
			instance = null;
		}

		/// <inheritdoc />
		public override void Uninstall()
		{
			//Not possible on car
		}

		/// <inheritdoc />
		public override void ResetToDefault(bool uninstall = false)
		{
			//Not possible on car
		}

		public PartEventListenerCollection GetEventListeners(PartEvent.Time eventTime, PartEvent.Type Type)
		{
			return new PartEventListenerCollection();
		}

		public new void AddEventListener(PartEvent.Time eventTime, PartEvent.Type Type, Action action)
		{
			//Not possible on car
		}
	}
}