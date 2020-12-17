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
using ScriptEngine.Hosting;

namespace ScriptEngine.HostedScript
{
    public class EngineConfigProvider
    {
        private readonly ConfigurationProviders _providers;
        KeyValueConfig _currentConfig;

        public const string CONFIG_FILE_NAME = "oscript.cfg";
        public const string SYSTEM_LIB_KEY = "lib.system";
        public const string ADDITIONAL_LIB_KEY = "lib.additional";

        public EngineConfigProvider(ConfigurationProviders providers)
        {
            _providers = providers;
            UpdateConfig();
        }

        private void UpdateConfig()
        {
            _currentConfig = _providers.CreateConfig();
        }
        
        public KeyValueConfig ReadConfig()
        {
            UpdateConfig();
            return _currentConfig;
        }
        
    }
}
