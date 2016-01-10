using ScriptEngine.Environment;
using ScriptEngine.HostedScript;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace oscript
{
    static class ScriptFileHelper
    {
        public static string CustomConfigPath(string scriptPath)
        {
            var dir = Path.GetDirectoryName(scriptPath);
            var cfgPath = Path.Combine(dir, HostedScriptEngine.ConfigFileName);
            if (File.Exists(cfgPath))
                return cfgPath;
            else
                return null;
        }

        public static void OnBeforeScriptRead(HostedScriptEngine engine)
        {
            var cfg = engine.GetWorkingConfig();

            string openerEncoding = cfg["encoding.script"];
            if(!String.IsNullOrWhiteSpace(openerEncoding) && StringComparer.InvariantCultureIgnoreCase.Compare(openerEncoding, "default") != 0)
            {
                FileOpener.DefaultEncoding = Encoding.GetEncoding(openerEncoding);
            }
        }
    }
}
