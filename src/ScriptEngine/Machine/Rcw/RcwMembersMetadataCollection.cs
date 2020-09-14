/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScriptEngine.Machine.Rcw
{
    public class RcwMembersMetadataCollection<T> where T : RcwMemberMetadata
    {
        private readonly List<T> _collection;
        private readonly Dictionary<int, T> _dispIds;
        private readonly Dictionary<string, T> _names;

        public IReadOnlyDictionary<int, T> DispatchIds => new ReadOnlyDictionary<int, T>(_dispIds);

        public IReadOnlyDictionary<string, T> Names => new ReadOnlyDictionary<string, T>(_names);

        public T this[int index] => _collection[index];

        public RcwMembersMetadataCollection()
        {
            _collection = new List<T>();
            _dispIds = new Dictionary<int, T>();
            _names = new Dictionary<string, T>(StringComparer.InvariantCultureIgnoreCase);
        }

        public int IndexOf(T item) => _collection.IndexOf(item);
        
        public void Add(T item)
        {
            if (_dispIds.ContainsKey(item.DispatchId))
            {
                _names.Add(item.Name, _dispIds[item.DispatchId]);
                return;
            }

            _collection.Add(item);
            _dispIds.Add(item.DispatchId, item);
            _names.Add(item.Name, item);
        }

        public bool Contains(T item) => _collection.Contains(item);

        public int Count => _collection.Count;
    }
}