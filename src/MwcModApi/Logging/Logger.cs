using MSCLoader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using System.Reflection;
using System.Collections;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace MwcModApi.Tools
{
	public static class Logger
	{
		private static int maxLineLength = 62;

		private static Dictionary<string, LoggedMod> loggedModsMap;
		private static List<string> initLoggerNotCalledByAssemblyCache;

		public static void InitLogger(Mod mod, string fileName)
		{
			Assembly callingAssembly = Assembly.GetCallingAssembly();
			Setup(callingAssembly.GetName().Name, mod, fileName);
		}

		public static void InitLogger(Mod mod)
		{
			Assembly callingAssembly = Assembly.GetCallingAssembly();

			Setup(callingAssembly.GetName().Name, mod, mod.ID + ".log");
		}

		private static void Setup(string callingAssembly, Mod mod, string fileName)
		{
			if (loggedModsMap.ContainsKey(mod.ID)) {
				ModConsole.Warning("Logger already initialized for mod with ID '" + mod.ID + "'");
				return;
			}

			LoggedMod loggedMod = new LoggedMod(mod, fileName);
			loggedModsMap.Add(callingAssembly, loggedMod);
			InitFile(loggedMod);
		}

		public static void Info(string message)
		{
			Assembly callingAssembly = Assembly.GetCallingAssembly();
			New(callingAssembly.GetName().Name, LogMessageType.Info, message, "", null);
		}

		public static void Info(string message, string additionalInfo)
		{
			Assembly callingAssembly = Assembly.GetCallingAssembly();
			New(callingAssembly.GetName().Name, LogMessageType.Info, message, additionalInfo, null);
		}

		public static void Info(string message, Exception ex)
		{
			Assembly callingAssembly = Assembly.GetCallingAssembly();
			New(callingAssembly.GetName().Name, LogMessageType.Info, message, "", ex);
		}

		public static void Info(string message, string additionalInfo, Exception ex)
		{
			Assembly callingAssembly = Assembly.GetCallingAssembly();
			New(callingAssembly.GetName().Name, LogMessageType.Info, message, additionalInfo, ex);
		}

		public static void Warning(string message)
		{
			Assembly callingAssembly = Assembly.GetCallingAssembly();
			New(callingAssembly.GetName().Name, LogMessageType.Warning, message, "", null);
		}

		public static void Warning(string message, string additionalInfo)
		{
			Assembly callingAssembly = Assembly.GetCallingAssembly();
			New(callingAssembly.GetName().Name, LogMessageType.Warning, message, additionalInfo, null);
		}

		public static void Warning(string message, Exception ex)
		{
			Assembly callingAssembly = Assembly.GetCallingAssembly();
			New(callingAssembly.GetName().Name, LogMessageType.Warning, message, "", ex);
		}

		public static void Warning(string message, string additionalInfo, Exception ex)
		{
			Assembly callingAssembly = Assembly.GetCallingAssembly();
			New(callingAssembly.GetName().Name, LogMessageType.Warning, message, additionalInfo, ex);
		}

		public static void Error(string message)
		{
			Assembly callingAssembly = Assembly.GetCallingAssembly();
			New(callingAssembly.GetName().Name, LogMessageType.Warning, message, "", null);
		}

		public static void Error(string message, string additionalInfo)
		{
			Assembly callingAssembly = Assembly.GetCallingAssembly();
			New(callingAssembly.GetName().Name, LogMessageType.Warning, message, additionalInfo, null);
		}

		public static void Error(string message, Exception ex)
		{
			Assembly callingAssembly = Assembly.GetCallingAssembly();
			New(callingAssembly.GetName().Name, LogMessageType.Warning, message, "", ex);
		}

		public static void Error(string message, string additionalInfo, Exception ex)
		{
			Assembly callingAssembly = Assembly.GetCallingAssembly();
			New(callingAssembly.GetName().Name, LogMessageType.Warning, message, additionalInfo, ex);
		}

		public static void New(LogMessageType logMessageType, string message)
		{
			Assembly callingAssembly = Assembly.GetCallingAssembly();
			New(callingAssembly.GetName().Name, logMessageType, message, "", null);
		}

		public static void New(LogMessageType logMessageType, string message, string additionalInfo)
		{
			Assembly callingAssembly = Assembly.GetCallingAssembly();
			New(callingAssembly.GetName().Name, logMessageType, message, additionalInfo, null);
		}

		public static void New(LogMessageType logMessageType, string message, Exception ex)
		{
			Assembly callingAssembly = Assembly.GetCallingAssembly();
			New(callingAssembly.GetName().Name, logMessageType, message, "", ex);
		}

		public static void New(LogMessageType logMessageType, string message, string additionalInfo, Exception ex)
		{
			Assembly callingAssembly = Assembly.GetCallingAssembly();
			New(callingAssembly.GetName().Name, logMessageType, message, additionalInfo, ex);
		}

		private static void New(
			string callingAssemblyName, 
			LogMessageType logMessageType, 
			string message,
			string additionalInfo, 
			Exception ex
			)
		{
			WriteLogEntry(callingAssemblyName, logMessageType, message, additionalInfo, ex);
		}

		private static void WriteLogEntry(
			string callingAssemblyName, 
			LogMessageType logMessageType, 
			string message, 
			string additionalInfo,
			Exception ex
			)
		{
			//If InitLogger wasn't called, warn once and default to printing message to ModConsole
			if (!loggedModsMap.TryGetValue(callingAssemblyName, out LoggedMod loggedMod)) {
				if (!initLoggerNotCalledByAssemblyCache.Contains(callingAssemblyName)) {
					initLoggerNotCalledByAssemblyCache.Add(callingAssemblyName);
					ModConsole.Error("Logger was not initialized by assembly with name '" + callingAssemblyName +
					                 "'. Logging will default to printing limited info to ModConsole!");
				}

				switch (logMessageType) 
				{
					case LogMessageType.Info:
						ModConsole.Log(message);
						break;
					case LogMessageType.Warning:
						ModConsole.Warning(message);
						break;
					case LogMessageType.Error:
						ModConsole.Error(message);
						break;

				}
				return;
			}

			using (var sw = new StreamWriter(loggedMod.FilePath, true)) {
				var errorLogLine = AddBaseLogLine(message, logMessageType);
				if (additionalInfo != "") {
					errorLogLine = AddAdditionalInfoLine(errorLogLine, additionalInfo);
				}

				if (ex != null && ex.Message != "") {
					errorLogLine = AddExceptionLine(errorLogLine, ex);
				}

				errorLogLine += Environment.NewLine;
				sw.Write(errorLogLine);
			}
		}

		private static void InitFile(LoggedMod loggedMod)
		{
			var modsInstalled = GetModsInstalled();
			var baseInformation =
				$@"╔{GenerateHeader(" Environment ")}
║ Steam:        {ModLoader.CheckSteam().ToXY("Yes", "No")}
║ OS:           {GetOperatingSystem()}
╠{GenerateHeader(" Mod ")}
║ Name:         {loggedMod.Mod.Name}
║ Version:      {loggedMod.Mod.Version}
║ Author:       {loggedMod.Mod.Author}
╠{GenerateHeader(" ModLoader ")}
║ Version:      {ModLoader.MSCLoader_Ver}
║ Experimental: {ModLoader.experimental}
╠{GenerateHeader(" Mods ")}
║
{modsInstalled}
╚{GenerateHeader("")}
";
			using (var streamWriter = new StreamWriter(loggedMod.FilePath, false)) {
				streamWriter.Write(baseInformation);
			}
		}

		private static string GenerateHeader(string description, char headerLine = '═')
		{
			var header = "════";
			header += description;
			header += new string(headerLine, (maxLineLength - header.Length));
			return header;
		}

		private static string GetOperatingSystem()
		{
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

			if (registryKey == null) {
				return "Unavailable";
			}

			return "Build: " + registryKey.GetValue("CurrentBuildNumber").ToString();
		}

		private static string GetModsInstalled()
		{
			var modsInstalled = "";
			foreach (var mod in ModLoader.LoadedMods) {
				// Ignore MSCLoader.
				if (mod.ID == "MSCLoader_Console" || mod.ID == "MSCLoader_Settings")
					continue;
				var modLine = string.Format(
					"║ [{0}] ID: {1} Name: {2} Version: {3}" + Environment.NewLine,
					mod.isDisabled.ToXY(" DISABLED ", " ACTIVE "),
					mod.ID,
					mod.Name,
					mod.Version
				);
				modsInstalled += modLine;
				if (maxLineLength < modLine.Length) {
					maxLineLength = modLine.Length;
				}
			}

			modsInstalled += "║";
			return modsInstalled;
		}

		private static string AddBaseLogLine(string message, LogMessageType logMessageType)
		{
			DateTime dateTime = DateTime.Now;
			string formattedDateTime = dateTime.ToString("G", CultureInfo.CreateSpecificCulture("de-DE"));
			return $"[{formattedDateTime}] [{logMessageType}] {message}";
		}

		private static string AddAdditionalInfoLine(string errorLogLine, string info)
		{
			errorLogLine += $"{Environment.NewLine}=> Additional infos: {info}";

			return errorLogLine;
		}

		private static string AddExceptionLine(string errorLogLine, Exception ex)
		{
			if (ex != null) {
				errorLogLine += $"{Environment.NewLine}=> Exception message: {ex.Message}";
			}

			return errorLogLine;
		}

		public static void LoadCleanup()
		{
			maxLineLength = 62;
			loggedModsMap = new Dictionary<string, LoggedMod>();
			initLoggerNotCalledByAssemblyCache = new List<string>();
		}
	}
}