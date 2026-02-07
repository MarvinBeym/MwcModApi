using HutongGames.PlayMaker;
using MSCLoader;
using MwcModApi.GlobalEvent;
using MwcModApi.Tools;
using UnityEngine;

namespace MwcModApi.Caching
{
	/// <summary>
	/// Utility class for everything related to the car, all cached for high performance.
	/// </summary>
	public class CarH
	{
		private static GameObject _car;
		private static Drivetrain _drivetrain;
		private static AxisCarController _axisController;
		private static FsmBool _electricsOk;
		private static GameObject _electricity;
		private static FsmString _playerCurrentVehicle;

		private static PlayMakerFSM starterFsm;

		internal static void Init()
		{
			starterFsm = Cache.Find("CORRIS/Simulation/STARTERxCorris").FindFsm("Starter");
			starterFsm.FindState("Running").AddActionAsLast(() =>
			{
				GlobalEventSystem.GetInstance().GetEventListeners(GlobalEventType.EngineRunning).InvokeAll();
			});

			starterFsm.FindState("Stall engine").AddActionAsLast(() =>
			{
				GlobalEventSystem.GetInstance().GetEventListeners(GlobalEventType.EngineStalled).InvokeAll();
			});
		}

		/// <summary>
		/// Returns if the car is currently running (rpm above 20).
		/// </summary>
		public static bool running => starterFsm?.ActiveStateName == "Running";

		/// <summary>
		/// Returns if the player is currently sitting in the car (drive mode).
		/// </summary>
		public static bool playerInCar => playerCurrentVehicle == "Corris";

		/// <summary>
		/// Returns the current vehicle the player is in (drive mode).
		/// </summary>
		public static string playerCurrentVehicle
		{
			get
			{
				if (_playerCurrentVehicle != null) return _playerCurrentVehicle.Value;
				_playerCurrentVehicle = FsmVariables.GlobalVariables.FindFsmString("PlayerCurrentVehicle");

				return _playerCurrentVehicle.Value;
			}
		}

		/// <summary>
		/// Returns the cars electricity object.
		/// </summary>
		public static GameObject electricity
		{
			get
			{
				if (_electricity != null) return _electricity;
				_electricity = car.FindChild("Simulation/Electricity").gameObject;

				return _electricity;
			}
		}

		/// <summary>
		/// Returns if the cars power is currently on.
		/// </summary>
		public static bool hasPower
		{
			get
			{
				if (_electricsOk != null) return _electricsOk.Value;
				var carElectricsPower = PlayMakerFSM.FindFsmOnGameObject(electricity, "Power");
				_electricsOk = carElectricsPower.FsmVariables.FindFsmBool("ElectricsOK");
				return _electricsOk.Value;
			}
		}

		/// <summary>
		/// Returns the UnityCar AxisCarController object of the car.
		/// </summary>
		public static AxisCarController axisCarController
		{
			get
			{
				if (_axisController != null) return _axisController;
				_axisController = car.GetComponent<AxisCarController>();

				return _axisController;
			}
		}

		/// <summary>
		/// Returns the UnityCar Drivetrain object of the car.
		/// </summary>
		public static Drivetrain drivetrain
		{
			get
			{
				if (_drivetrain != null) return _drivetrain;
				_drivetrain = car.GetComponent<Drivetrain>();

				return _drivetrain;
			}
		}

		/// <summary>
		/// Returns the car GameObject object.
		/// </summary>
		public static GameObject car
		{
			get
			{
				if (_car != null) return _car;
				_car = Cache.Find("CORRIS");

				return _car;
			}
		}

		/// <summary>
		/// Called when the MwcModApi mod loads to cleanup static data
		/// </summary>
		public static void LoadCleanup()
		{
			_car = null;
			_drivetrain = null;
			_axisController = null;
			_electricsOk = null;
			_electricity = null;
			_playerCurrentVehicle = null;
			starterFsm = null;
		}
	}
}