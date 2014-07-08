using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine;

namespace ScriptEngine.Compiler
{
    class SymbolScope
    {
        Dictionary<string, int> _variableNumbers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        List<VariableInfo> _variables = new List<VariableInfo>();

        Dictionary<string, int> _methodsNumbers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        List<MethodInfo> _methods = new List<MethodInfo>();

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
                throw new SymbolNotFoundException(name);
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
                throw new SymbolNotFoundException(name);
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
            return DefineVariable(name, SymbolType.Variable);
        }

        public int DefineVariable(string name, SymbolType symbolType)
        {
            if (!IsVarDefined(name))
            {

                var newIdx = _variables.Count;
                _variableNumbers[name] = newIdx;

                _variables.Add(new VariableInfo()
                {
                    Index = newIdx,
                    Type = symbolType
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

        public string GetVariableName(int number)
        {
            return _variableNumbers.First(x => x.Value == number).Key;
        }

        public int VariableCount 
        {
            get
            {
                return _variables.Count;
            }
        }

        public bool IsDynamicScope 
        { 
            get; 
            set; 
        }
    }

    class SymbolNotFoundException : CompilerException
    {
        public SymbolNotFoundException(string symbol) : base(string.Format("Неизвестный символ: {0}", symbol))
        {

        }
    }

}
