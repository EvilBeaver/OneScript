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
        Dictionary<int, MethodUsageData> _methodsData = new Dictionary<int, MethodUsageData>();

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

        public int DefineMethod(string name, MethodUsageData methodUsageData)
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
            _methodsData.Add(newIndex, methodUsageData);

            return newIndex;
        }

        public void SetMethodAlias(int methodNumber, string alias)
        {
            if(IsMethodDefined(alias))
                throw new ArgumentException("Метод (" + alias + ") уже определен");

            if(!Utils.IsValidIdentifier(alias))
                throw new ArgumentException("Некорректное имя (" + alias + ")");

            _methodsNumbers.Add(alias, methodNumber);
        }

        public void SetVariableAlias(int variableNumber, string alias)
        {
            if (IsVarDefined(alias))
                throw new ArgumentException("Переменная (" + alias + ") уже определена");

            if (!Utils.IsValidIdentifier(alias))
                throw new ArgumentException("Некорректное имя (" + alias + ")");

            _variableNumbers.Add(alias, variableNumber);
            
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
                return _methodsData.Count;
            }
        }

        public MethodUsageData GetMethodUsageData(int methodNumber)
        {
            return _methodsData[methodNumber];
        }
    }
}
