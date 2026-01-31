using System;
using System.Data.SqlTypes;
using HutongGames.PlayMaker;
using MwcModApi.Tools;
using UnityEngine;

namespace MwcModApi.Caching
{
	/// <summary>
	/// Utility class for everything related to the player.
	/// </summary>
	public class Player
	{
		private static GameObject _player;
		private static FsmFloat _money;
		private static GameObject _hand;
		private static PlayMakerFSM _handPickUp;
		private static FsmGameObject _currentObjectInHand;


		/// <summary>
		/// Returns the player GameObject
		/// </summary>
		public static GameObject player
		{
			get
			{
				if (_player == null) {
					_player = Cache.Find("PLAYER");
				}

				return _player;
			}
		}

		/// <summary>
		/// Returns the hand GameObject. This GameObject contains the FSM that handles object pickup
		/// </summary>
		public static GameObject hand
		{
			get
			{
				if (_hand == null)
				{
					_hand = Cache.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera/1Hand_Assemble/Hand");
				}

				return _hand;
			}
		}

		/// <summary>
		/// Returns the FSM that handles object pickup
		/// </summary>
		public static PlayMakerFSM handPickUp
		{
			get
			{
				if (_handPickUp == null)
				{
					_handPickUp = hand.FindFsm("PickUp");
				}

				return _handPickUp;
			}
		}

		/// <summary>
		/// Returns the current GameObject the player is holding in their hand
		/// </summary>
		public static GameObject currentObjectInHand
		{
			get
			{
				if (_currentObjectInHand == null)
				{
					_currentObjectInHand = handPickUp.FsmVariables.FindFsmGameObject("PickedObject");
				}

				return _currentObjectInHand.Value;
			}
		}

		/// <summary>
		/// Returns if the player is holding the passed GameObject in their hands
		/// </summary>
		/// <param name="gameObject">The GameObject to check</param>
		/// <returns>True if player is currently holding part in hands, otherwise false</returns>
		public static bool IsHolding(GameObject gameObject)
		{
			return gameObject.transform.root == player.transform;
		}

		/// <summary>
		/// Returns the current amount of money the player has.
		/// </summary>
		public static float money
		{
			get
			{
				if (_money != null) return _money.Value;
				_money = PlayMakerGlobals.Instance.Variables.FindFsmFloat("PlayerMoney");
				return (float) Math.Round(_money.Value, 1);
			}
			set
			{
				if (_money != null) _money.Value = value;
				_money = PlayMakerGlobals.Instance.Variables.FindFsmFloat("PlayerMoney");
				_money.Value = value;
			}
		}

		/// <summary>
		/// Called when the MwcModApi mod loads to cleanup static data
		/// </summary>
		public static void LoadCleanup()
		{
			_money = null;
			_player = null;
			_money = null;
			_hand = null;
			_handPickUp = null; 
			_currentObjectInHand = null;
		}
	}
}