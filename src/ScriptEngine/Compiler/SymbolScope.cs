using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine;

namespace ScriptEngine.Compiler
{
    class SymbolScope : ISymbolScope
    {
        Dictionary<string, int> _variableNumbers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        List<VariableInfo> _variables = new List<VariableInfo>();

        Dictionary<string, int> _methodsNumbers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        List<MethodInfo> _methods = new List<MethodInfo>();

        #region ISymbolScope Members

        public MethodInfo GetMethod(string name)
        {
            var num = GetMethodNumber(name);
            return _methods[num];
        }

        public MethodInfo GetMethod(int number)
        {
            return _methods[number];
        }

        public int GetVariableNumber(string name)
        {
            try
            {
                return _variableNumbers[name];
            }
            catch (KeyNotFoundException)
            {
                throw new SymbolNotFoundException();
            }
        }

        public VariableInfo GetVariable(int number)
        {
            return _variables[number];
        }

        public int GetMethodNumber(string name)
        {
            try
            {
                return _methodsNumbers[name];
            }
            catch (KeyNotFoundException)
            {
                throw new SymbolNotFoundException();
            }
        }

        public bool IsVarDefined(string name)
        {
            return _variableNumbers.ContainsKey(name);
        }

        public bool IsMethodDefined(string name)
        {
            return _methodsNumbers.ContainsKey(name);
        }

        public int DefineVariable(string name)
        {
            if (!IsVarDefined(name))
            {

                var newIdx = _variables.Count;
                _variableNumbers[name] = newIdx;

                _variables.Add(new VariableInfo()
                {
                    Index = newIdx,
                    Type = SymbolType.Variable
                });

                return newIdx;
            }
            else
            {
                throw new InvalidOperationException("Symbol already defined in the scope");
            }
        }

        public int DefineVariable(VariableDescriptor varSymbol)
        {
            if (!IsVarDefined(varSymbol.Identifier))
            {
                var newIdx = _variables.Count;
                _variableNumbers[varSymbol.Identifier] = newIdx;

                _variables.Add(new VariableInfo()
                {
                    Index = newIdx,
                    Type = varSymbol.Type
                });

                return newIdx;
            }
            else
            {
                throw new InvalidOperationException("Symbol already defined in the scope");
            }
        }

        public int DefineMethod(MethodInfo method)
        {
            if (!IsMethodDefined(method.Name))
            {
                int newIdx = _methods.Count;
                _methods.Add(method);
                _methodsNumbers[method.Name] = newIdx;
                return newIdx;
            }
            else
            {
                throw new InvalidOperationException("Symbol already defined in the scope");
            }
        }

        public int VariableCount 
        {
            get
            {
                return _variables.Count;
            }
        }

        #endregion
    }

    interface ISymbolScope
    {
        MethodInfo GetMethod(string name);
        MethodInfo GetMethod(int number);
        int GetVariableNumber(string name);
        VariableInfo GetVariable(int number);
        int GetMethodNumber(string name);
        bool IsVarDefined(string name);
        bool IsMethodDefined(string name);
        int DefineVariable(string name);
        int DefineVariable(VariableDescriptor varSymbol);
        int DefineMethod(MethodInfo method);
        int VariableCount { get; }
    }

    class SymbolNotFoundException : CompilerException
    {
        public SymbolNotFoundException() : base("Symbol not found")
        {

        }
    }

}
