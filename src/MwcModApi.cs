using MSCLoader;
using UnityEngine;

namespace MwcModApi
{
	public class MwcModApi : Mod
	{
		public override string ID => "MwcModApi"; // Your (unique) mod ID 
		public override string Name => "MwcModApi"; // Your mod name
		public override string Author => "DonnerPlays"; // Name of the Author (your name)
		public override string Version => "1.0"; // Version
		public override string Description => ""; // Short description of your mod 
		public override Game SupportedGames => Game.MyWinterCar;

		public override void ModSetup()
		{
			SetupFunction(Setup.OnLoad, Mod_OnLoad);
			SetupFunction(Setup.ModSettings, Mod_Settings);
		}

		private void Mod_Settings()
		{
			// All settings should be created here. 
			// DO NOT put anything that isn't settings or keybinds in here!
		}

		private void Mod_OnLoad()
		{
			// Called once, when mod is loading after game is fully loaded
		}
	}
}
