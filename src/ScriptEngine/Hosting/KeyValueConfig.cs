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
    public class KeyValueConfig
    {
        private readonly Dictionary<string, ConfigurationValue> _values = new Dictionary<string, ConfigurationValue>(StringComparer.InvariantCultureIgnoreCase);

        public void Merge(IDictionary<string, string> source, IConfigProvider sourceProvider)
        {
            foreach (var keyValue in source)
            {
                if (string.IsNullOrWhiteSpace(keyValue.Key))
                    throw BadKeyException(keyValue.Key);

                _values[keyValue.Key] = new ConfigurationValue(keyValue.Value, sourceProvider);
            }
        }

        public ConfigurationValue GetEntry(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw BadKeyException(key);

            _values.TryGetValue(key, out var value);
            return value;
        }

        public string this[string key]
        {
            get
            {
                var entry = GetEntry(key);
                return entry?.RawValue;
            }
        }

        private static ArgumentException BadKeyException(string key)
        {
            return new ArgumentException($"wrong config key format: {key}");
        }

    }
}
