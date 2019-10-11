/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections;
using System.Collections.Generic;

namespace OneScript.Language
{
    public class IdentifiersTrie<T> : IDictionary<string, T>
    {
        private class TrieNode
        {
            public char charL;
            public char charU;
            public T value;
            public TrieNode sibl;
            public TrieNode next;

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

        private readonly TrieNode _root;

        public IdentifiersTrie()
        {
            _root = new TrieNode();
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
                        value = default(T),
                        sibl = node.sibl
                    };
                    node.sibl = key;
                    key.next = new TrieNode();
                }
                node = key.next;
            }

            node.value = val;
        }

        public bool ContainsKey(string key)
        {
            throw new System.NotImplementedException();
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

            return node.value;
        }

        public T this[string index]
        {
            get => Get(index);
            set => Add(index, value);
        }

        public ICollection<string> Keys { get; }
        public ICollection<T> Values { get; }

        public bool TryGetValue(string str, out T value)
        {
            var node = _root;
            foreach (char ch in str)
            {
                var key = node.Find(ch);
                if (key == null)
                {
                    value = default(T);
                    return false;
                }

                node = key.next;
            }

            value = node.value;
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
