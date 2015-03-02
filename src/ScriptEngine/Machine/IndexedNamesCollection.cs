using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    class IndexedNamesCollection
    {
        private List<string> _names = new List<string>();
        private Dictionary<string, int> _nameIndexes = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

        public bool TryGetIdOfName(string name, out int id)
        {
            return _nameIndexes.TryGetValue(name, out id);
        }

        public string GetName(int id)
        {
            return _names[id];
        }

        public int RegisterName(string name)
        {
            int id = _names.Count;
            _nameIndexes.Add(name, id);
            _names.Add(name);

            return id;
        }
    }
}
