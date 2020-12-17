/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace ScriptEngine.Hosting
{
    public class ConfigurationProviders
    {
        private List<Func<IDictionary<string, string>>> _providers = new List<Func<IDictionary<string, string>>>();

        public void Add(Func<IDictionary<string, string>> configGetter)
        {
            _providers.Add(configGetter);
        }

        public KeyValueConfig CreateConfig()
        {
            var cfg = new KeyValueConfig();
            foreach (var provider in _providers)
            {
                cfg.Merge(provider());
            }

            return cfg;
        }
    }
}