/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Diagnostics;

namespace OneScript.Language
{
    public class LexemTrie<T>
    {
        private class TrieNode
        {
            public char UCase;
            public char LCase;

            public TrieNode next;
            public TrieNode sibling;
            public T value;
        }

        private static int _alphabetLength;
        private TrieNode[] _alphabet;
        private int _count;

        static LexemTrie()
        {
            var ru = "абвгдеёжзийклмнопрстуфхцчшщьыъэюя";
            var en = "abcdefghijklmnopqrstuvwxyz";
            var symbols = @"+-*/\()[].,<>=;?%0123456789";

            var all = symbols +
                      ru +
                      ru.ToUpper() +
                      en +
                      en.ToUpper();

            _alphabetLength = all.Length;
        }

        public LexemTrie()
        {
            _alphabet = new TrieNode[_alphabetLength];
        }

        public int Count => _count;
        
        private static int GetIndex(char c)
        {
            var code = (int) c;

            if (code >= 1040 && code <= 1103)
            {
                return code - 960;
            }

            if (code >= 40 && code <= 57)
            {
                return code - 39;
            }

            if (code >= 59 && code <= 63)
            {
                return code - 40;
            }

            if (code >= 65 && code <= 93)
            {
                return code - 41;
            }

            if (code >= 97 && code <= 122)
            {
                return code - 44;
            }

            switch (c)
            {
                case '%':
                    return 0;
                case 'Ё':
                    return 79;
                case 'ё':
                    return 144;
            }

            return -1;
        }

        private TrieNode GetValueNode(string key)
        {
            var index = GetIndex(key[0]);
            if (index == -1)
                return null;

            var node = _alphabet[index];
            if (node == null)
            {
                node = new TrieNode();
                node.LCase = char.ToLower(key[0]);
                node.UCase = char.ToUpper(key[0]);
                _alphabet[GetIndex(node.LCase)] = node;
                _alphabet[GetIndex(node.UCase)] = node;
            }

            for (int i = 1; i < key.Length; i++)
            {
                var current = node;
                node = node.next;
                if (node == null)
                {
                    var newNode = new TrieNode();
                    newNode.LCase = char.ToLower(key[i]);
                    newNode.UCase = char.ToUpper(key[i]);
                    current.next = newNode;
                    node = newNode;
                }
                else if (node.LCase != key[i] && node.UCase != key[i])
                {
                    var insert = node.sibling;
                    while (insert != null)
                    {
                        if (insert.LCase == key[i] || insert.UCase == key[i])
                        {
                            node = insert;
                            break;
                        }

                        node = insert;
                        insert = insert.sibling;
                    }

                    if (insert == null)
                    {
                        var newNode = new TrieNode();
                        newNode.LCase = char.ToLower(key[i]);
                        newNode.UCase = char.ToUpper(key[i]);
                        node.sibling = newNode;
                        node = newNode;
                    }
                }
            }

            return node;
        }

        public void Add(string key, T value)
        {
            var node = GetValueNode(key);
            Debug.Assert(node != null);

            node.value = value;
            ++_count;
        }

        public T Get(string key)
        {
            var node = FindNode(key);
            if (node == null)
            {
                throw new KeyNotFoundException();
            }

            return node.value;
        }

        private TrieNode FindNode(string key)
        {
            var index = GetIndex(key[0]);
            if (index == -1 || _alphabet[index] == null)
                return null;

            var node = _alphabet[index];
            for (int i = 1; i < key.Length; i++)
            {
                node = node.next;
                if (node == null)
                    return null;

                while(node.LCase != key[i] && node.UCase != key[i])
                {
                    node = node.sibling;
                    if(node == null)
                        return null;
                }
            }

            return node;
        }

        public bool TryGetValue(string key, out T value)
        {
            var node = FindNode(key);
            if (node == null)
            {
                value = default(T);
                return false;
            }

            value = node.value;
            return true;
        }

        public T this[string key]
        {
            get => Get(key);
            set => Add(key, value);
        }
    }
}
