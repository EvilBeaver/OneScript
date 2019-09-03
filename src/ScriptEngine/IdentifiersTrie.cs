/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;

namespace ScriptEngine
{
    public class IdentifiersTrie<T>
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

        TrieNode root;

        public IdentifiersTrie() { root = new TrieNode(); }

        public void Add(string str, T val)
        {
            var node = root;
            foreach (char ch in str)
            {
                var key = node.Find(ch);
                if (key == null)
                {
                    key = new TrieNode
                    {
                        charL = char.ToLower(ch),
                        charU = char.ToUpper(ch),
                        value = default,
                        sibl = node.sibl
                    };
                    node.sibl = key;
                    key.next = new TrieNode();
                }
                node = key.next;
            }

            node.value = val;
        }

        public T Find(string str)
        {
            var node = root;
            foreach (char ch in str)
            {
                TrieNode key = node.Find(ch);
                if (key == null)
                    throw new KeyNotFoundException();

                node = key.next;
            }

            return node.value;
        }

        public bool TryFind(string str, out T value)
        {
            var node = root;
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

            value = node.value;
            return true;
        }
    }
}
