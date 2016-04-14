using OneScript.Core;
using OneScript.Runtime.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime
{
    class RuntimeValuesHolder : ISymbolScope, IRuntimeValueHolder
    {
        private List<IValue> _values = new List<IValue>();
        IndexedNamesCollection _names = new IndexedNamesCollection();

        IValueRef[] _variables;

        public IValue ValueOf(string nameOrAlias)
        {
            int output;
            if(!_names.TryGetIdOfName(nameOrAlias, out output))
                throw new InvalidOperationException("Wrong name");

            return ValueOf(output);
        }

        public IValue ValueOf(int index)
        {
            return ValueRefs[index].Value;
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
            int idx;
            if(_names.TryGetIdOfName(name, out idx))
            {
                return idx;
            }
            else
            {
                return SymbolScope.InvalidIndex;
            }
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

        public IValueRef[] ValueRefs
        {
            get
            {
                if (_variables == null)
                {
                    _variables = _values.Select(x => new GeneralValueRef(x)).ToArray();
                }

                return _variables;
            }
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
