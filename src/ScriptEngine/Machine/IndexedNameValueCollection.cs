using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace ScriptEngine.Machine
{
    /// <summary>
    /// Универсальный класс для коллекций доступных по имени и номеру.
    /// Уже создано много с похожим функционалом по всему проекту. Постепенно можно переводить сюда.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class IndexedNameValueCollection<T> : IEnumerable<T>
    {
        private readonly Dictionary<string, int> _nameIndex = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
        private readonly List<T> _values;

        public void Add(T item, string name)
        {
            if (_nameIndex.ContainsKey(name))
                throw new InvalidOperationException($"Name {name} already registered");

            var idx = _values.Count;
            _values.Add(item);
            _nameIndex[name] = idx;
        }

        public T this[int index] { get => _values[index]; set => _values[index] = value; }
        public T this[string name]
        {
            get => this[IndexOf(name)];
            set => this[IndexOf(name)] = value;
        }

        public void AddName(int index, string name)
        {
            if (index < 0 || index >= _values.Count)
                throw new ArgumentOutOfRangeException();

            _nameIndex[name] = index;
        }

        public int IndexOf(string name)
        {
            int idx;
            if (!_nameIndex.TryGetValue(name, out idx))
                return -1;

            return _nameIndex[name];
        }

        public bool TryGetValue(string name, out T result)
        {
            int idx;
            if(_nameIndex.TryGetValue(name, out idx))
            {
                result = _values[idx];
                return true;
            }
            else
            {
                result = default(T);
                return false;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
