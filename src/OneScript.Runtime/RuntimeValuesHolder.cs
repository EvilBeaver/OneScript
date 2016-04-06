using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime
{
    class RuntimeValuesHolder : ISymbolScope
    {
        private List<IValue> _values = new List<IValue>();
        IndexedNamesCollection _names = new IndexedNamesCollection();

        public IValue ValueOf(string nameOrAlias)
        {
            int output;
            if(!_names.TryGetIdOfName(nameOrAlias, out output))
                throw new InvalidOperationException("Wrong name");

            return _values[output];
        }

        public IValue ValueOf(int index)
        {
            return _values[index];
        }

        public int GetMethodNumber(string name)
        {
            return -1;
        }

        public IEnumerable<string> GetMethodSymbols()
        {
            return new string[0];
        }

        public int GetVariableNumber(string name)
        {
            return -1;
        }

        public IEnumerable<string> GetVariableSymbols()
        {
            return _names;
        }

        public bool IsMethodDefined(string name)
        {
            return false;
        }

        public bool IsVarDefined(string name)
        {
            return _names.HasName(name);
        }

        public int MethodCount
        {
            get { return 0; }
        }

        public int VariableCount
        {
            get { return _values.Count; }
        }

        public int DefineVariable(string name)
        {
            throw new InvalidOperationException();
        }

        public int DefineVariable(string name, IValue value)
        {
            int idx = _names.RegisterName(name);
            _values.Add(value);

            return idx;
        }

        public int DefineMethod(string name)
        {
            throw new NotSupportedException();
        }

        public void SetMethodAlias(int methodNumber, string alias)
        {
            throw new NotSupportedException();
        }

        public void SetVariableAlias(int variableNumber, string alias)
        {
            _names.RegisterAlias(variableNumber, alias);
        }
    }
}
