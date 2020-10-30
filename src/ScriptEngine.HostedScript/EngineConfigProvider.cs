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
    class EngineConfigProvider
    {
        KeyValueConfig _currentConfig;

        readonly string _customConfigFilePath;

        public const string CONFIG_FILE_NAME = "oscript.cfg";
        public const string SYSTEM_LIB_KEY = "lib.system";
        public const string ADDITIONAL_LIB_KEY = "lib.additional";

        public EngineConfigProvider(string customConfigFile)
        {
            _customConfigFilePath = customConfigFile;
            ReadConfig();
        }

        public static string DefaultConfigFilePath()
        {
            string asmLocation;
            asmLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            if (String.IsNullOrEmpty(asmLocation))
                asmLocation = System.Reflection.Assembly.GetEntryAssembly().Location;

            if (String.IsNullOrEmpty(asmLocation))
                asmLocation = System.Reflection.Assembly.GetCallingAssembly().Location;

            if (String.IsNullOrEmpty(asmLocation))
                asmLocation = AppDomain.CurrentDomain.BaseDirectory;

            var assemblyPath = System.IO.Path.GetDirectoryName(asmLocation);
            var configFile = System.IO.Path.Combine(assemblyPath, CONFIG_FILE_NAME);
            if (System.IO.File.Exists(configFile)) {
                LibraryResolver.TraceLoadLibrary(String.Format("Файл настроек по умолчанию для загрузки библиотек, расположен {0}", configFile));
                return configFile;
            } else {
                LibraryResolver.TraceLoadLibrary(String.Format("Файл настроек по умолчанию для загрузки библиотек, НЕ НАЙДЕН в каталоге {0} ", assemblyPath));
                return null;
            }
                
        }

        private void ReadDefaultConfig()
        {
            var defaultCfgPath = DefaultConfigFilePath();
            if(defaultCfgPath != null)
            {
                var dict = ReadConfigFile(defaultCfgPath);
                _currentConfig = new KeyValueConfig(dict);
            }
            else
            {
                _currentConfig = new KeyValueConfig(new Dictionary<string, string>());
            }
        }

        private static Dictionary<string, string> ReadConfigFile(string configPath)
        {
            Dictionary<string, string> conf = new Dictionary<string, string>();
            using (var reader = new StreamReader(configPath, true))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (String.IsNullOrWhiteSpace(line) || line[0] == '#')
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

        private static void ExpandRelativePaths(Dictionary<string, string> conf, string configFile)
        {
            string sysDir = null;
            conf.TryGetValue(SYSTEM_LIB_KEY, out sysDir);

            var confDir = System.IO.Path.GetDirectoryName(configFile);
            if (sysDir != null && !System.IO.Path.IsPathRooted(sysDir))
            {
                sysDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(confDir, sysDir));
                conf[SYSTEM_LIB_KEY] = sysDir;
            }

            string additionals;
            if (conf.TryGetValue(ADDITIONAL_LIB_KEY, out additionals))
            {
                var fullPaths = additionals.Split(new[]{";"}, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(x => Path.GetFullPath(Path.Combine(confDir, x)));
                conf[ADDITIONAL_LIB_KEY] = String.Join(";",fullPaths);
            }
        }

        private void ReadCustomConfig()
        {
            if (!String.IsNullOrWhiteSpace(_customConfigFilePath) && File.Exists(_customConfigFilePath))
            {
                var dict = ReadConfigFile(_customConfigFilePath);
                _currentConfig.Merge(dict);
            }
        }

        private void ReadEnvironmentOverrides()
        {
            var env = System.Environment.GetEnvironmentVariable("OSCRIPT_CONFIG");
            if(env == null)
                return;

            var paramList = new FormatParametersList(env);
            _currentConfig.Merge(paramList.ToDictionary());
        }

        public KeyValueConfig ReadConfig()
        {
            ReadDefaultConfig();
            ReadCustomConfig();
            ReadEnvironmentOverrides();

            return _currentConfig;
        }
        
    }
}
