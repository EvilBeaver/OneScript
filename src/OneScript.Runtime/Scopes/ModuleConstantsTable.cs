using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneScript.Core;
using OneScript.Language;

namespace OneScript.Runtime.Scopes
{
    public class ModuleConstantsTable
    {
        private List<ConstDefinition> _constants = new List<ConstDefinition>();

        public int GetConstIndex(ConstDefinition constValue)
        {
            var idx = _constants.IndexOf(constValue);
            if (idx < 0)
            {
                idx = _constants.Count;
                _constants.Add(constValue);
            }

            return idx;
        }

        public int Count
        {
            get
            {
                return _constants.Count;
            }
        }

        public ConstDefinition this[int index]
        {
            get
            {
                return _constants[index];
            }
        }
    }
}
