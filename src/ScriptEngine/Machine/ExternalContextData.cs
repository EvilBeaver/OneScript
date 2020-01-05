/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;

namespace ScriptEngine.Machine
{
    public class ExternalContextData : IDictionary<string, IValue>
    {
        private readonly Dictionary<string, IValue> _data = new Dictionary<string, IValue>();

        public void Add(string key, IValue value)
        {
            CheckKeyName(key);
            _data.Add(key, value);
        }

        private static void CheckKeyName(string key)
        {
            if (String.IsNullOrEmpty(key))
                throw new ArgumentException("Property name is empty");

            if (!Utils.IsValidIdentifier(key))
                throw new ArgumentException("Invalid property name");
        }

        public bool ContainsKey(string key)
        {
            return _data.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return _data.Keys; }
        }

        public bool Remove(string key)
        {
            return _data.Remove(key);
        }

        public bool TryGetValue(string key, out IValue value)
        {
            return _data.TryGetValue(key, out value);
        }

        public ICollection<IValue> Values
        {
            get { return _data.Values; }
        }

        public IValue this[string key]
        {
            get
            {
                return _data[key];
            }
            set
            {
                CheckKeyName(key);
                _data[key] = value;
            }
        }

        public void Add(KeyValuePair<string, IValue> item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            _data.Clear();
        }

        public bool Contains(KeyValuePair<string, IValue> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<string, IValue>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public int Count
        {
            get { return _data.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<string, IValue> item)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<string, IValue>> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
