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
        private readonly Dictionary<string, string> _values = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        public KeyValueConfig()
        { 
        }

        public KeyValueConfig(IDictionary<string, string> source)
        {
            Merge(source);
        }

        public void Merge(IDictionary<string, string> source)
        {
            foreach (var keyValue in source)
            {
                this[keyValue.Key] = keyValue.Value;
            }
        }

        public string this[string key]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw BadKeyException(key);

                _values.TryGetValue(key, out var value);
                
                return value;

            }
            private set
            {
                if (String.IsNullOrWhiteSpace(key))
                    throw BadKeyException(key);

                _values[key] = value;
            }
        }

        private static ArgumentException BadKeyException(string key)
        {
            return new ArgumentException($"wrong config key format: {key}");
        }

    }
}
