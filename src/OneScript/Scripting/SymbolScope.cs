using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    public class SymbolScope
    {
        Dictionary<string, int> _variableNumbers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, int> _methodsNumbers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public static int InvalidIndex
        {
            get { return -1; }
        }

        public int DefineVariable(string name)
        {
            if (IsVarDefined(name))
            {
                throw new ArgumentException("Переменная (" + name + ") уже определена");
            }

            if (!Utils.IsValidIdentifier(name))
            {
                throw new ArgumentException("Некорректное имя (" + name + ")");
            }

            int newIndex = VariableCount;
            _variableNumbers.Add(name, newIndex);

            return newIndex;
        }

        public bool IsVarDefined(string name)
        {
            return _variableNumbers.ContainsKey(name);
        }

        public int GetVariableNumber(string name)
        {
            int idx;
            if (_variableNumbers.TryGetValue(name, out idx))
            {
                return idx;
            }
            else
            {
                return InvalidIndex;
            }
        }

        public int VariableCount
        {
            get
            {
                return _variableNumbers.Count;
            }
        }

        public int DefineMethod(string name)
        {
            if(IsMethodDefined(name))
            {
                throw new ArgumentException("Метод (" + name + ") уже определен");
            }

            if (!Utils.IsValidIdentifier(name))
            {
                throw new ArgumentException("Некорректное имя (" + name + ")");
            }
            
            int newIndex = MethodCount;
            _methodsNumbers.Add(name, newIndex);

            return newIndex;
        }

        public bool IsMethodDefined(string name)
        {
            return _methodsNumbers.ContainsKey(name);
        }

        public int GetMethodNumber(string name)
        {
            int idx;
            if (_methodsNumbers.TryGetValue(name, out idx))
            {
                return idx;
            }
            else
            {
                return InvalidIndex;
            }
        }

        public int MethodCount
        {
            get
            {
                return _methodsNumbers.Count;
            }
        }
    }
}
