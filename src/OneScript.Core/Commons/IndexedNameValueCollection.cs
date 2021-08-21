/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OneScript.Commons
{
    /// <summary>
    /// Универсальный класс для коллекций доступных по имени и номеру.
    /// Уже создано много с похожим функционалом по всему проекту. Постепенно можно переводить сюда.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IndexedNameValueCollection<T> : IEnumerable<T>
    {
        private readonly Dictionary<string, int> _nameIndex = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
        private readonly List<T> _values = new List<T>();

        public int Add(T item, string name)
        {
            if (_nameIndex.ContainsKey(name))
                throw new InvalidOperationException($"Name {name} already registered");

            var idx = _values.Count;
            _values.Add(item);
            _nameIndex[name] = idx;
            return idx;
        }

        public int Add(T item, string name, string alias)
        {
            var index = _values.Count;
            Add(item, name);
            
            if(!string.IsNullOrEmpty(alias))
                AddName(index, alias);
            
            return index;
        }

        public T this[int index]
        {
            get => _values[index];
            set => _values[index] = value;
        }

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

        public int Count => _values.Count; 
        
        public int IndexOf(string name)
        {
            if (!_nameIndex.TryGetValue(name, out var index))
                return -1;

            return index;
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

        public IEnumerable<KeyValuePair<string, int>> GetIndex()
        {
            return _nameIndex.AsEnumerable();
        }
        
        public void RemoveValue(string name)
        {
            var idx = IndexOf(name);
            if (idx == -1)
            {
                throw new InvalidOperationException("Name is not belong to index");
            }

            var indices = _nameIndex
                .Where(x => x.Value == idx)
                .Select(x => x.Key);

            foreach (var referencedName in indices)
            {
                _nameIndex.Remove(referencedName);
            }
            
            Reindex();
        }

        private void Reindex()
        {
            var nameMap = _nameIndex
                .OrderBy(x => x.Value)
                .Select(x => new
                {
                    x.Key,
                    Value = _values[x.Value]
                }).ToList();

            Clear();

            foreach (var item in nameMap)
            {
                var newIndex = _values.Count;
                _values.Add(item.Value);
                _nameIndex.Add(item.Key, newIndex);
            }
        }

        public void RemoveName(string name)
        {
            _nameIndex.Remove(name);
            Reindex();
        }

        public void Clear()
        {
            _nameIndex.Clear();
            _values.Clear();
        }
    }
}
