/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptEngine.Hosting
{
    public class ConfigurationProviders
    {
        private readonly List<IConfigProvider> _providers = new List<IConfigProvider>();

        public void Add(IConfigProvider source)
        {
            _providers.Add(source);
        }

        public KeyValueConfig CreateConfig()
        {
            var cfg = new KeyValueConfig();
            foreach (var provider in _providers)
            {
                var values = provider.Load();
                if (values != null && values.Count > 0)
                {
                    cfg.Merge((IDictionary<string, string>)values, provider);
                }
            }

            return cfg;
        }
    }
}
