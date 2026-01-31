using System;
using System.Data.SqlTypes;
using HutongGames.PlayMaker;

namespace MwcModApi.Caching
{
	/// <summary>
	/// Utility class for everything related to the player.
	/// </summary>
	public class Player
	{
		private static FsmFloat _money;

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
		}
	}
}