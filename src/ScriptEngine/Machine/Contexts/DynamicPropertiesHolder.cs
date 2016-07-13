/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    public class DynamicPropertiesHolder
    {
        private readonly Dictionary<string, int> _propNumbers = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

        public int RegisterProperty(string name)
        {
            if (_propNumbers.ContainsKey(name))
            {
                return _propNumbers[name];
            }
            else
            {
                if (!IsValidIdentifier(name))
                {
                    throw new RuntimeException("Неверное значение аргумента");
                }

                var idx = _propNumbers.Count;
                _propNumbers.Add(name, idx);
                return idx;
            }
        }

        public void RemoveProperty(string name)
        {
            _propNumbers.Remove(name);
        }

        public void ReorderPropertyNumbers()
        {
            var sorted = _propNumbers.OrderBy(x => x.Value).Select(x => x.Key).ToArray();
            _propNumbers.Clear();
            for (int i = 0; i < sorted.Length; i++)
            {
                _propNumbers.Add(sorted[i], i);
            }
        }

        public void ClearProperties()
        {
            _propNumbers.Clear();
        }

        public int GetPropertyNumber(string name)
        {
            try
            {
                return _propNumbers[name];
            }
            catch (KeyNotFoundException)
            {
                throw RuntimeException.PropNotFoundException(name);
            }
        }

        public IEnumerable<KeyValuePair<string, int>> GetProperties()
        {
            return _propNumbers.AsEnumerable();
        }

        private bool IsValidIdentifier(string name)
        {
            return Utils.IsValidIdentifier(name);
        }

    }
}
