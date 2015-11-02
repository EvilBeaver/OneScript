using OneScript.Scopes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime.Scopes
{
    public class ModuleVariableTable : IEnumerable<SymbolBinding>
    {
        private List<SymbolBinding> _vars = new List<SymbolBinding>();

        public int Add(SymbolBinding binding)
        {
            int idx = _vars.Count;
            _vars.Add(binding);
            return idx;
        }

        public int GetVariableIndex(SymbolBinding binding)
        {
            var idx = _vars.IndexOf(binding);
            if (idx < 0)
                idx = Add(binding);

            return idx;
        }

        public int Count
        {
            get
            {
                return _vars.Count;
            }
        }

        public SymbolBinding this[int index]
        {
            get
            {
                return _vars[index];
            }
        }

        #region IEnumerable<SymbolBinding> Members

        public IEnumerator<SymbolBinding> GetEnumerator()
        {
            return _vars.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

}
