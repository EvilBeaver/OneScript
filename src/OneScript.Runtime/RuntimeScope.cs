using OneScript.Core;
using OneScript.Runtime.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime
{
    public class RuntimeScope : ISymbolScope
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

        public IEnumerable<string> GetMethodSymbols()
        {
            return _symbols.GetMethodSymbols();
        }

        public IEnumerable<string> GetVariableSymbols()
        {
            return _symbols.GetVariableSymbols();
        }

        public bool IsMethodDefined(string name)
        {
            return _symbols.IsMethodDefined(name);
        }

        public bool IsVarDefined(string name)
        {
            return _symbols.IsVarDefined(name);
        }

        public int MethodCount
        {
            get { return _symbols.MethodCount; }
        }

        public int VariableCount
        {
            get { return _symbols.VariableCount; }
        }

        public int DefineVariable(string name) { throw new NotSupportedException("Scope is read only"); }
        public int DefineMethod(string name) { throw new NotSupportedException("Scope is read only"); }
        public void SetMethodAlias(int methodNumber, string alias) { throw new NotSupportedException("Scope is read only"); }
        public void SetVariableAlias(int variableNumber, string alias) { throw new NotSupportedException("Scope is read only"); }

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
