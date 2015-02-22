using System;
using System.Collections.Generic;
using OneScript.ComponentModel;
using OneScript.Core;

namespace OneScript.Compiler
{
    public class SymbolScope
    {
        Dictionary<string, int> _variableNumbers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, int> _methodsNumbers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        Dictionary<int, MethodSignatureData> _methodsData = new Dictionary<int, MethodSignatureData>();

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

        public int DefineMethod(string name, MethodSignatureData methodUsageData)
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

        public MethodSignatureData GetMethodUsageData(int methodNumber)
        {
            return _methodsData[methodNumber];
        }

        public static SymbolScope ExtractFromContext(IRuntimeContextInstance context)
        {
            var scope = new SymbolScope();

            ReadProperties(scope, context);
            ReadMethods(scope, context);

            return scope;
        }

        private static void ReadProperties(SymbolScope scope, IRuntimeContextInstance context)
        {
            var propCount = context.GetPropCount();
            for (int i = 0; i < propCount; i++)
            {
                var index = scope.DefineVariable(context.GetPropertyName(i, NameRetrievalMode.Name));
                var alias = context.GetPropertyName(i, NameRetrievalMode.OnlyAlias);
                if(!String.IsNullOrWhiteSpace(alias))
                    scope.SetVariableAlias(index, alias);
            }
        }

        private static void ReadMethods(SymbolScope scope, IRuntimeContextInstance context)
        {
            int count = context.GetMethodsCount();
            if (count == 0)
                return;

            var methods = MethodSignatureExtractor.Extract(context);
            for (int i = 0; i < count; i++)
            {
                var index = scope.DefineMethod(context.GetMethodName(i, NameRetrievalMode.Name), methods[i]);
                var alias = context.GetMethodName(i, NameRetrievalMode.OnlyAlias);
                if (!String.IsNullOrWhiteSpace(alias))
                    scope.SetMethodAlias(index, alias);
            }
        }
    }
}
