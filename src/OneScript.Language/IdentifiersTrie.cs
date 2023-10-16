/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections;
using System.Collections.Generic;

namespace OneScript.Language
{
    public class IdentifiersTrie<T> : IDictionary<string, T>
    {
        private readonly TrieNode _root = new TrieNode();
        
        private class TrieNode
        {
            public char charL;
            public char charU;
            public TrieNode sibl;
            public TrieNode next;

            private T _value;

            public T Value
            {
                get => _value;
                set
                {
                    HasValue = true;
                    _value = value;
                }
            }
            
            public bool HasValue { get; private set; }
            
            public TrieNode Find(char ch)
            {
                var node = sibl;
                while (node != null)
                {
                    if (node.charL == ch || node.charU == ch)
                        return node;
                    node = node.sibl;
                }
                return null;
            }

        }

        public void Add(string str, T val)
        {
            var node = _root;
            foreach (char ch in str)
            {
                var key = node.Find(ch);
                if (key == null)
                {
                    key = new TrieNode
                    {
                        charL = char.ToLower(ch),
                        charU = char.ToUpper(ch),
                        Value = default(T),
                        sibl = node.sibl
                    };
                    node.sibl = key;
                    key.next = new TrieNode();
                }
                node = key.next;
            }

            node.Value = val;
        }

        public bool ContainsKey(string key)
        {
            var node = _root;
            foreach (char ch in key)
            {
                var keyNode = node.Find(ch);
                if (keyNode == null)
                {
                    return false;
                }
                node = keyNode.next;
            }

            return node.next == null && node.HasValue;
        }

        public bool Remove(string key)
        {
            throw new System.NotImplementedException();
        }

        public T Get(string str)
        {
            var node = _root;
            foreach (char ch in str)
            {
                TrieNode key = node.Find(ch);
                if (key == null)
                    throw new KeyNotFoundException();

                node = key.next;
            }
            
            if (!node.HasValue)
                throw new KeyNotFoundException();

            return node.Value;
        }

        public T this[string index]
        {
            get => Get(index);
            set => Add(index, value);
        }

        public ICollection<string> Keys => throw new NotSupportedException();
        public ICollection<T> Values => throw new NotSupportedException();

        public bool TryGetValue(string str, out T value)
        {
            var node = _root;
            foreach (char ch in str)
            {
                var key = node.Find(ch);
                if (key == null)
                {
                    value = default;
                    return false;
                }

                node = key.next;
            }

            if (!node.HasValue)
            {
                value = default;
                return false;
            }
            
            value = node.Value;
            return true;
        }

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, T> item)
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, T> item)
        {
            throw new System.NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, T> item)
        {
            throw new System.NotImplementedException();
        }

        public int Count { get; }
        public bool IsReadOnly { get; }
    }
}
