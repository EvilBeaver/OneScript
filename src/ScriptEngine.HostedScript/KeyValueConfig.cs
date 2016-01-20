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
using System.Text;

namespace ScriptEngine.HostedScript
{
    public class KeyValueConfig
    {
        private Dictionary<string, string> _values = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        public KeyValueConfig()
        { 
        }

        public KeyValueConfig(Dictionary<string, string> source)
        {
            Merge(source);
        }

        public void Merge(Dictionary<string, string> source)
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
                if (String.IsNullOrWhiteSpace(key))
                    throw BadKeyException(key);

                string value = null;
                _values.TryGetValue(key, out value);
                
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
            return new ArgumentException(String.Format("wrong config key format: {0}", key));
        }

    }
}
