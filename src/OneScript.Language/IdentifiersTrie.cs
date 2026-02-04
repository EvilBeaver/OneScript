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
            internal char charL;
            internal char charU;
            internal TrieNode sibl;
            internal TrieNode next;

            internal T value;

            internal bool hasValue;

            internal TrieNode() { }
            internal TrieNode(char ch)
                { charL = char.ToLower(ch); charU = char.ToUpper(ch); }

            internal TrieNode Find(char ch)
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
            TrieNode key = node;
            foreach (char ch in str)
            {
                if (node == null)
                {
                    node = new TrieNode(ch);
                    key.next = node;
                    key = node;
                    node = null;
                }
                else
                {
                    TrieNode last = node;
                    key = node;
                    while (key != null && key.charL != ch && key.charU != ch)
                    {
                        last = key;
                        key = key.sibl;
                    }
                    if (key == null)
                    {
                        key = new TrieNode(ch);
                        last.sibl = key;
                    }
                    node = key.next;
                }
            }

            key.value = val;
            key.hasValue = true;
        }

        public bool ContainsKey(string str)
        {
            return TryGetValue(str, out _);
        }

        public bool Remove(string key)
        {
            throw new System.NotImplementedException();
        }

        public T Get(string str)
        {
            return TryGetValue(str, out var value) ? value
                : throw new KeyNotFoundException();
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
            TrieNode key = _root;
            var node = key.sibl;
            foreach (char ch in str)
            {
                while (node != null && node.charL != ch && node.charU != ch)
                {
                    node = node.sibl;
                }
                if (node == null)
                {
                    value = default;
                    return false;
                }

                key = node;
                node = key.next;
            }

            if (key.hasValue)
            {
                value = key.value;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
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
