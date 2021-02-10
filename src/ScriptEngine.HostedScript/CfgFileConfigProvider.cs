/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScriptEngine.HostedScript
{
    public class CfgFileConfigProvider : IConfigProvider
    {
        public const string CONFIG_FILE_NAME = "oscript.cfg";
        
        public string FilePath { get; set; }
        
        public Func<IDictionary<string, string>> GetProvider()
        {
            var localCopy = FilePath;
            return () => ReadConfigFile(localCopy);
        }
        
        private static IDictionary<string, string> ReadConfigFile(string configPath)
        {
            var conf = new Dictionary<string, string>();
            using (var reader = new StreamReader(configPath, true))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line) || line[0] == '#')
                        continue;

                    var keyValue = line.Split(new[] { '=' }, 2);
                    if (keyValue.Length != 2)
                        continue;

                    conf[keyValue[0].Trim()] = keyValue[1].Trim();
                }
            }

            ExpandRelativePaths(conf, configPath);

            return conf;
        }

        private static void ExpandRelativePaths(IDictionary<string, string> conf, string configFile)
        {
            string sysDir = null;
            conf.TryGetValue(OneScriptLibraryOptions.SYSTEM_LIBRARY_DIR, out sysDir);

            var confDir = System.IO.Path.GetDirectoryName(configFile);
            if (sysDir != null && !System.IO.Path.IsPathRooted(sysDir))
            {
                sysDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(confDir, sysDir));
                conf[OneScriptLibraryOptions.SYSTEM_LIBRARY_DIR] = sysDir;
            }

            string additionals;
            if (conf.TryGetValue(OneScriptLibraryOptions.ADDITIONAL_LIBRARIES, out additionals))
            {
                var fullPaths = additionals.Split(new[]{";"}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Path.GetFullPath(Path.Combine(confDir, x)));
                conf[OneScriptLibraryOptions.ADDITIONAL_LIBRARIES] = string.Join(";",fullPaths);
            }
        }
    }
}