using OneScript.Core;
using OneScript.Runtime.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime
{
    public class RuntimeScope
    {
        private SymbolScope _symbols;
        private IRuntimeContextInstance _target;
        private IValueRef[] _variables;

        private RuntimeScope()
	    {
	    }

        public int GetVariableNumber(string name)
        {
            return _symbols.GetVariableNumber(name);
        }

        public int GetMethodNumber(string name)
        {
            return _symbols.GetMethodNumber(name);
        }

        public IValue ValueOf(string name)
        {
            var index = GetVariableNumber(name);
            if (index == SymbolScope.InvalidIndex)
                throw new ArgumentException("Undefined name " + name, "name");

            return ValueOf(index);
        }

        public IValue ValueOf(int index)
        {
            return _variables[index].Value;
        }

        public IValueRef[] ValueRefs
        {
            get
            {
                return _variables;
            }
        }

        private void InitVariables()
        {
            // TODO: в scope могут быть не только свойства контекста, пока минимальный функционал для прохождения теста
            _variables = new IValueRef[_symbols.VariableCount];
            for (int i = 0; i < _variables.Length-1; i++)
            {
                _variables[i] = new PropertyValueRef(_target, i);
            }
        }

        // static factory method
        public static RuntimeScope FromContext(IRuntimeContextInstance source)
        {
            SymbolScope symbols = SymbolScope.ExtractFromContext(source);

            var result = new RuntimeScope()
            {
                _symbols = symbols,
                _target = source
            };

            result.InitVariables();

            return result;
        }
    }
}
