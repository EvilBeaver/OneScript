/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Hosting;

namespace ScriptEngine.HostedScript
{
    public class EngineConfigProvider
    {
        private readonly ConfigurationProviders _providers;
        KeyValueConfig _currentConfig;

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
