using MSCLoader;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MwcModApi.Tools
{
	public static class Helper
	{
		/// <summary>
		/// Combines several file system paths to a valid path using "/"
		/// </summary>
		/// <param name="paths">An array of paths/dirs</param>
		/// <returns>The constructed path</returns>
		public static string CombinePaths(params string[] paths)
		{
			if (paths == null) {
				throw new ArgumentNullException(nameof(paths));
			}

			return paths.Aggregate(Path.Combine);
		}

		/// <summary>
		/// Combines several file system paths to a valid path using "/".
		/// Also creates the directory if it does not exist yet
		/// </summary>
		/// <param name="paths"></param>
		/// <returns></returns>
		public static string CombinePathsAndCreateIfNotExists(params string[] paths)
		{
			string path = CombinePaths(paths);
			if (!Directory.Exists(path)) {
				Directory.CreateDirectory(path);
			}

			return path;
		}

		/// <summary>
		/// Load an asset bundle from file.
		/// Shows error message with option to close game on failure
		/// </summary>
		/// <param name="mod">Your mod instance</param>
		/// <param name="fileName">The assetBundle file name</param>
		/// <returns></returns>
		public static AssetBundle LoadAssetBundle(Mod mod, string fileName)
		{
			try {
				return LoadAssets.LoadBundle(mod, fileName);
			} catch {
				var message = $"AssetBundle file '{fileName}' could not be loaded";
				ModConsole.Error(message);
				ModUI.ShowYesNoMessage($"{message}\n\nClose Game? - RECOMMENDED", ExitGame);
			}

			return null;
		}

		/// <summary>
		/// Load an asset bundle from an embedded resource.
		/// Shows error message with option to close game on failure.
		/// </summary>
		/// <param name="bundleName">The name of the bundle example: Namespace.Folder.assetBundle.unity3d</param>
		/// <returns></returns>
		public static AssetBundle LoadAssetBundle(string bundleName)
		{
			try {
				using (Stream manifestResourceStream = Assembly.GetCallingAssembly().GetManifestResourceStream(bundleName))
				{
					byte[] data = manifestResourceStream != null 
						? new byte[manifestResourceStream.Length] 
						: throw new Exception($"<b>LoadAssetBundle() Error:</b> Resource {bundleName} doesn't exist." + Environment.NewLine);
					manifestResourceStream.Read(data, 0, data.Length);
					return AssetBundle.CreateFromMemoryImmediate(data);
				}
			} catch {
				var message = $"AssetBundle bundle '{bundleName}' could not be loaded";
				ModConsole.Error(message);
				ModUI.ShowYesNoMessage($"{message}\n\nClose Game? - RECOMMENDED", ExitGame);
			}

			return null;
		}

		/// <summary>
		/// Force closes the game
		/// </summary>
		public static void ExitGame()
		{
			Application.Quit();
		}

		/// <summary>
		/// Compare if two Vector3 are near each other
		/// </summary>
		/// <param name="positionToCheck">The first Vector3</param>
		/// <param name="position">The second Vector3</param>
		/// <param name="minimumDistance">The minimum distance to return true</param>
		/// <returns></returns>
		public static bool CheckCloseToPosition(Vector3 positionToCheck, Vector3 position, float minimumDistance)
		{
			try {
				return Vector3.Distance(positionToCheck, position) <= minimumDistance;
			} catch {
				return false;
			}
		}

		/// <summary>
		/// Loads a save from a file or returns a new instance of the save on error
		/// </summary>
		/// <typeparam name="T">The save class type</typeparam>
		/// <param name="mod">Your mod instance</param>
		/// <param name="saveFilePath">The path where the save should be loaded from</param>
		/// <returns>A loaded save or a new instance of your save class</returns>
		public static T LoadSaveOrReturnNew<T>(Mod mod, string saveFilePath) where T : new()
		{
			var path = Path.Combine(ModLoader.GetModSettingsFolder(mod), saveFilePath);

			T save;

			if (!File.Exists(path)) {
				save = new T();
			} else {
				save = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));

				if (save == null) {
					save = new T();
				}
			}

			return save;
		}

		/// <summary>
		/// Loads a part from the supplied AssetBundle, adds the correct tag for parts and fixes the name
		/// </summary>
		/// <param name="assetsBundle">Your loaded AssetBundle</param>
		/// <param name="prefabName">The name of the prefab that should be loaded from the AssetBundle</param>
		/// <param name="name">The name the loaded part should have</param>
		/// <param name="addClone">Adds (Clone) at the end of the part, required otherwise the name may be shown malformed when looked at.</param>
		/// <returns></returns>
		internal static GameObject LoadPartAndSetName(
			AssetBundle assetsBundle,
			string prefabName,
			string name,
			bool addClone = true
		)
		{
			var gameObject = GameObject.Instantiate(assetsBundle.LoadAsset(prefabName) as GameObject);
			gameObject.SetNameLayerTag(name + (addClone ? "(Clone)" : ""), "PART", "Parts");

			return gameObject;
		}

		/// <summary>
		/// Load a new Sprite
		/// </summary>
		/// <param name="current">The current sprite</param>
		/// <param name="data">The data of the new sprite</param>
		/// <param name="pivotX">Pivot position on X-Axis</param>
		/// <param name="pivotY">Pivot position on Y-Axis</param>
		/// <param name="pixelsPerUnit">Pixels per unit</param>
		/// <returns></returns>
		public static Sprite LoadNewSprite(
			Sprite current,
			byte[] data,
			float pivotX = 0.5f,
			float pivotY = 0.5f,
			float pixelsPerUnit = 100.0f
		)
		{
			var spriteTexture = LoadTexture(data);
			if (!spriteTexture) {
				return current;
			}

			return Sprite.Create(
				spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height),
				new Vector2(pivotX, pivotY), pixelsPerUnit
			);
		}

		/// <summary>
		/// Load a new Sprite from file
		/// </summary>
		/// <param name="current">The current sprite</param>
		/// <param name="filePath">The filepath of the Sprite to load</param>
		/// <param name="pivotX">Pivot position on X-Axis</param>
		/// <param name="pivotY">Pivot position on Y-Axis</param>
		/// <param name="pixelsPerUnit">Pixels per unit</param>
		/// <returns></returns>
		public static Sprite LoadNewSprite(
			Sprite current,
			string filePath,
			float pivotX = 0.5f,
			float pivotY = 0.5f,
			float pixelsPerUnit = 100.0f
		)
		{
			if (File.Exists(filePath) && Path.GetExtension(filePath) == ".png") {
				return LoadNewSprite(current, File.ReadAllBytes(filePath), pivotX, pivotY, pixelsPerUnit);
			}

			return current;
		}

		/// <summary>
		/// Load a texture from a byte array
		/// </summary>
		/// <param name="data">The byte array containing the texture to load</param>
		/// <returns></returns>
		public static Texture2D LoadTexture(byte[] data)
		{
			var Tex2D = new Texture2D(2, 2);
			return Tex2D.LoadImage(data) ? Tex2D : null;
		}

		/// <summary>
		/// Find a PlayMakerFSM component on a GameObject
		/// </summary>
		/// <param name="gameObject">The GameObject to search on</param>
		/// <param name="fsmName">The name of the PlayMakerFSM component to find</param>
		/// <returns></returns>
		public static PlayMakerFSM FindFsmOnGameObject(GameObject gameObject, string fsmName)
		{
			foreach (PlayMakerFSM fSM in gameObject.GetComponents<PlayMakerFSM>()) {
				if (fSM.FsmName == fsmName) {
					return fSM;
				}
			}

			return null;
		}

		/// <summary>
		/// Alternative to the now deprecated ModLoader.GetMod() function
		/// </summary>
		/// <param name="modId">ID of the mod</param>
		/// <returns>Mod or null</returns>
		public static Mod GetMod(string modId, bool ignoreEnabled = false)
		{
			if (ModLoader.IsModPresent(modId)) {
				return ModLoader.LoadedMods.FirstOrDefault(mod => mod.ID.Equals(modId) && !mod.isDisabled);
			}

			return ignoreEnabled ? ModLoader.LoadedMods.FirstOrDefault(mod => mod.ID.Equals(modId)) : null;
		}
	}
}