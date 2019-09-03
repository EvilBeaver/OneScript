/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;

namespace ScriptEngine
{
    public class IdentifiersTrie
    {
        private class TrieNode
        {
            public char charL;
            public char charU;
            public int value;
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

        public void Add(string str, int val)
        {
            TrieNode node = root;
            foreach (char ch in str)
            {
                TrieNode key = node.Find(ch);
                if (key == null)
                {
                    key = new TrieNode() { charL = Char.ToLower(ch), charU = Char.ToUpper(ch), value = -1 };
                    key.sibl = node.sibl;
                    node.sibl = key;
                    key.next = new TrieNode();
                }
                node = key.next;
            }

            node.value = val;
        }

        public int Find(string str)
        {
            TrieNode node = root;
            foreach (char ch in str)
            {
                TrieNode key = node.Find(ch);
                if (key == null)
                    return -1;

                node = key.next;
            }

            return node.value;
        }
    }
}
