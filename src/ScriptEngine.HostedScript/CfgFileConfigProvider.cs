/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using ScriptEngine.Hosting;

namespace ScriptEngine.HostedScript
{
    public class CfgFileConfigProvider : IConfigProvider
    {
        public const string CONFIG_FILE_NAME = "oscript.cfg";
        
        public string FilePath { get; set; }

        public bool Required { get; set; }

        public string SourceId => FilePath;

        public IReadOnlyDictionary<string, string> Load()
        {
            return (IReadOnlyDictionary<string, string>)ReadConfigFile(FilePath);
        }

        public string ResolveRelativePath(string path)
        {
            var confDir = Path.GetDirectoryName(FilePath);
            return Path.Combine(confDir, path);
        }
        
        private IDictionary<string, string> ReadConfigFile(string configPath)
        {
            var conf = new Dictionary<string, string>();
            StreamReader reader;
            try
            {
                reader = new StreamReader(configPath, true);
            }
            catch (IOException)
            {
                if (!Required)
                    return conf;

                throw;
            }
            
            using (reader)
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

            return conf;
        }
    }
}
