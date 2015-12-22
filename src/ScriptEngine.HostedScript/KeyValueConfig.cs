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
    class KeyValueConfig
    {
        private Dictionary<string, string> _values = new Dictionary<string, string>();

        private KeyValueConfig()
        { 
        }

        public string this[string key]
        {
            get
            {
                if (String.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("wrong config key format");

                string value = null;
                _values.TryGetValue(key, out value);
                
                return value;

            }
        }

        public static KeyValueConfig Read(StreamReader reader)
        {
            var conf = new KeyValueConfig();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (String.IsNullOrWhiteSpace(line) || line[0] == '#')
                    continue;

                var keyValue = line.Split(new[] { '=' }, 2);
                if (keyValue.Length != 2)
                    continue;

                conf._values[keyValue[0].Trim()] = keyValue[1].Trim();
            }

            return conf;
        }
        
        public static KeyValueConfig Read(string configPath)
        {
            KeyValueConfig conf;
            using(var reader = new StreamReader(configPath, true))
            {
                conf = Read(reader);
            }

            return conf;
        }
    }
}
