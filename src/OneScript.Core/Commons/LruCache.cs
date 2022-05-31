/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace OneScript.Contexts.Internal
{
    public class LruCache<TKey, TValue>
    {
        private readonly int _capacity;

        private readonly Dictionary<TKey, LinkedListNode<CacheItem>> _index =
            new Dictionary<TKey, LinkedListNode<CacheItem>>();

        private readonly LinkedList<CacheItem> _list = new LinkedList<CacheItem>();

        public LruCache(int capacity)
        {
            _capacity = capacity;
        }

        public bool IsEmpty() => _index.Count == 0;

        public void Clear()
        {
            _index.Clear();
            _list.Clear();
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
        {
            if (_index.TryGetValue(key, out var listNode))
            {
                _list.Remove(listNode);
                _list.AddFirst(listNode);
                return listNode.Value.Value;
            }

            if (_index.Count == _capacity)
            {
                var keyOfOld = _list.Last.Value.Key;
                _index.Remove(keyOfOld);
                _list.RemoveLast();
            }

            var newItem = _list.AddFirst(CacheItem.Create(key, factory(key)));
            _index[key] = newItem;
            return newItem.Value.Value;
        }

        private class CacheItem
        {
            public static CacheItem Create(TKey key, TValue value) =>
                new CacheItem(key, value);
            
            private CacheItem(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            } 
            
            public TKey Key { get; }
             
            public TValue Value { get; }
        }
    }
}