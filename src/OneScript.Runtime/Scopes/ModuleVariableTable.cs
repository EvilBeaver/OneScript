using OneScript.Scopes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime.Scopes
{
    public class ModuleVariableTable
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
    }

}
