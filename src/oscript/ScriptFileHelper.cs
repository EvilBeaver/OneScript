/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
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
            if(!String.IsNullOrWhiteSpace(openerEncoding))
            {
                if (StringComparer.InvariantCultureIgnoreCase.Compare(openerEncoding, "default") == 0)
                    engine.Loader.ReaderEncoding = FileOpener.SystemSpecificEncoding();
                else
                    engine.Loader.ReaderEncoding = Encoding.GetEncoding(openerEncoding); 
            }
        }
    }
}
