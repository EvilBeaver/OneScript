using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime
{
    class IndexedNamesCollection : IEnumerable<string>
    {
        private List<string> _names = new List<string>();
        private Dictionary<string, int> _nameIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public bool TryGetIdOfName(string name, out int id)
        {
            return _nameIndexes.TryGetValue(name, out id);
        }

        public string GetName(int id)
        {
            return _names[id];
        }

        public bool HasName(string name)
        {
            return _nameIndexes.ContainsKey(name);
        }

        public int RegisterName(string name)
        {
            int id = _names.Count;
            _nameIndexes.Add(name, id);
            _names.Add(name);

            return id;
        }

        public void RegisterAlias(int nameId, string alias)
        {
            if (nameId < 0 || nameId >= _names.Count)
                throw new ArgumentOutOfRangeException("nameId");

            _nameIndexes.Add(alias, nameId);
        }

        public int Count
        {
            get
            {
                return _names.Count;
            }
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _nameIndexes.Keys.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}