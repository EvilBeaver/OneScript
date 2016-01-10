using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript
{
    class EngineConfigProvider
    {
        KeyValueConfig _currentConfig;
        string _customConfigFilePath;

        const string CONFIG_FILE_NAME = "oscript.cfg";
        
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

            var assemblyPath = System.IO.Path.GetDirectoryName(asmLocation);
            var configFile = System.IO.Path.Combine(assemblyPath, CONFIG_FILE_NAME);
            if (System.IO.File.Exists(configFile))
                return configFile;
            else
                return null;
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

            return conf;
        }

        private void ReadCustomConfig()
        {
            if (!String.IsNullOrWhiteSpace(_customConfigFilePath) && File.Exists(_customConfigFilePath))
            {
                var dict = ReadConfigFile(_customConfigFilePath);
                _currentConfig.Merge(dict);
            }
        }

        public KeyValueConfig ReadConfig()
        {
            ReadDefaultConfig();
            ReadCustomConfig();

            return _currentConfig;
        }
    }
}
