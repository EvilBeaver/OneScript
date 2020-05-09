using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
            _collection.Add(item);
            _dispIds.Add(item.DispatchId, item);
            _names.Add(item.Name, item);
        }

        public bool Contains(T item) => _collection.Contains(item);

        public int Count => _collection.Count;
    }
}