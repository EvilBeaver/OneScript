/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace OneScript.Commons
{
    public class IndexedNamesCollection
    {
        private readonly List<string> _names = new List<string>();
        private readonly Dictionary<string, int> _nameIndexes = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

        public bool TryGetIdOfName(string name, out int id)
        {
            return _nameIndexes.TryGetValue(name, out id);
        }

        public string GetName(int id)
        {
            return _names[id];
        }

        public int RegisterName(string name, string alias = null)
        {
            System.Diagnostics.Debug.Assert(name != null);

            int id = _names.Count;
            _nameIndexes.Add(name, id);
            _names.Add(name);
            if (alias != null)
                _nameIndexes.Add(alias, id);

            return id;
        }
    }
}
