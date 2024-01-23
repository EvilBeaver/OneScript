/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OneScript.Rcw
{
    public class RcwMembersMetadataCollection<T> where T : RcwMemberMetadata
    {
        private readonly List<T> _collection = new List<T>();
        private readonly Dictionary<int, T> _indexByDispId = new Dictionary<int, T>();
        private readonly Dictionary<string, T> _indexByName = new Dictionary<string, T>(StringComparer.InvariantCultureIgnoreCase);

        public IReadOnlyDictionary<int, T> ByDispatchId => _indexByDispId;

        public IReadOnlyDictionary<string, T> ByName => _indexByName;

        public T this[int index] => _collection[index];

        public int IndexOf(T item) => _collection.IndexOf(item);
        
        public void Add(T item)
        {
            if (_indexByDispId.TryGetValue(item.DispatchId, out var method))
            {
                if (method.Name != item.Name)
                {
                    // добавляют метод, который известен нам, с тем же dispId, но который имеет другое имя
                    _indexByName.Remove(method.Name); // известное нам старое имя этого dispId - инвалидируем
                    
                    // добавляемый метод поместим в индекс диспатчей и имен
                    _indexByDispId[item.DispatchId] = item;
                    _indexByName[item.Name] = item;
                }
                else
                {
                    _indexByName[item.Name] = item;
                }
                
                return;
            }

            _collection.Add(item);
            _indexByDispId.Add(item.DispatchId, item);
            _indexByName.Add(item.Name, item);
        }

        public bool Contains(T item) => _collection.Contains(item);

        public int Count => _collection.Count;
    }
}