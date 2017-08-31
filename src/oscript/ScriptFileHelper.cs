/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Net.Configuration;
using System.Reflection;
using System.Text;

using ScriptEngine.Environment;
using ScriptEngine.HostedScript;

namespace oscript
{
	internal static class ScriptFileHelper
	{
		public static bool CodeStatisticsEnabled { get; private set; }

		public static string CodeStatisticsFileName { get; private set; }

		public static string CustomConfigPath(string scriptPath)
		{
			var dir = Path.GetDirectoryName(scriptPath);
			var cfgPath = Path.Combine(dir, HostedScriptEngine.ConfigFileName);
			return File.Exists(cfgPath) ? cfgPath : null;
		}

		public static void EnableCodeStatistics(string fileName)
		{
			CodeStatisticsEnabled = fileName != null;
			CodeStatisticsFileName = fileName;
		}

		// http://www.cookcomputing.com/blog/archives/000556.html
		public static bool SetAllowUnsafeHeaderParsing()
		{
			var aNetAssembly = Assembly.GetAssembly(typeof(SettingsSection));
			if (aNetAssembly == null) return false;

			var aSettingsType = aNetAssembly.GetType("System.Net.Configuration.SettingsSectionInternal");
			if (aSettingsType == null) return false;

			var anInstance = aSettingsType.InvokeMember("Section", BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic, null, null, new object[]
			{
			});
			if (anInstance == null) return false;

			var aUseUnsafeHeaderParsing = aSettingsType.GetField("useUnsafeHeaderParsing", BindingFlags.NonPublic | BindingFlags.Instance);
			if (aUseUnsafeHeaderParsing == null) return false;

			aUseUnsafeHeaderParsing.SetValue(anInstance, true);
			return true;
		}

		private static bool ConvertSettingValueToBool(string value, bool defaultValue = false)
		{
			if (value == null) return defaultValue;

			if (string.Compare(value, "true", StringComparison.InvariantCultureIgnoreCase) == 0) return true;
			if (string.Compare(value, "1", StringComparison.InvariantCultureIgnoreCase) == 0) return true;
			if (string.Compare(value, "yes", StringComparison.InvariantCultureIgnoreCase) == 0) return true;

			if (string.Compare(value, "false", StringComparison.InvariantCultureIgnoreCase) == 0) return false;
			if (string.Compare(value, "0", StringComparison.InvariantCultureIgnoreCase) == 0) return false;
			if (string.Compare(value, "no", StringComparison.InvariantCultureIgnoreCase) == 0) return false;

			return defaultValue;
		}

		public static void OnBeforeScriptRead(HostedScriptEngine engine)
		{
			var cfg = engine.GetWorkingConfig();

			var openerEncoding = cfg["encoding.script"];
			if (!string.IsNullOrWhiteSpace(openerEncoding))
				if (StringComparer.InvariantCultureIgnoreCase.Compare(openerEncoding, "default") == 0)
					engine.Loader.ReaderEncoding = FileOpener.SystemSpecificEncoding();
				else
					engine.Loader.ReaderEncoding = Encoding.GetEncoding(openerEncoding);

			var strictWebRequest = ConvertSettingValueToBool(cfg["http.strictWebRequest"]);
			if (!strictWebRequest)
				SetAllowUnsafeHeaderParsing();

			if (CodeStatisticsEnabled)
				engine.EnableCodeStatistics();
		}

		public static void OnAfterScriptExecute(HostedScriptEngine engine)
		{
			if (CodeStatisticsEnabled)
			{
				var codeStat = engine.GetCodeStatData();
				var statsWriter = new CodeStatWriter(CodeStatisticsFileName, CodeStatWriterType.JSON);
				statsWriter.Write(codeStat);
			}
		}
	}
}