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
        private List<IConfigProvider> _providers = new List<IConfigProvider>();
        private List<Func<IDictionary<string, string>>> _legacyProviders = new List<Func<IDictionary<string, string>>>();

        public void Add(IConfigProvider source)
        {
            _providers.Add(source);
        }

        [Obsolete("Используйте Add(IConfigProvider)")]
        public void Add(Func<IDictionary<string, string>> configGetter)
        {
            _legacyProviders.Add(configGetter);
        }

        public KeyValueConfig CreateConfig()
        {
            var cfg = new KeyValueConfig();
            foreach (var provider in _providers)
            {
                var values = provider.Load();
                cfg.Merge((IDictionary<string, string>)values, provider);
            }

            foreach (var legacy in _legacyProviders)
            {
                cfg.Merge(legacy());
            }

            return cfg;
        }
    }
}
