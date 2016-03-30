using System;
using System.Collections.Generic;
using OneScript.ComponentModel;
using OneScript.Core;

namespace OneScript.Runtime.Compiler
{
    public class SymbolScope : ISymbolScope
    {
        IndexedNamesCollection _variables = new IndexedNamesCollection();
        IndexedNamesCollection _methods = new IndexedNamesCollection();

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

            int newIndex = _variables.RegisterName(name);
            return newIndex;
        }

        public IEnumerable<string> GetVariableSymbols()
        {
            return _variables;
        }

        public bool IsVarDefined(string name)
        {
            return _variables.HasName(name);
        }

        public int GetVariableNumber(string name)
        {
            int idx;
            if (_variables.TryGetIdOfName(name, out idx))
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
                return _variables.Count;
            }
        }

        public IEnumerable<string> GetMethodSymbols()
        {
            return _methods;
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

            int newIndex = _methods.RegisterName(name);
            
            return newIndex;
        }

        public void SetMethodAlias(int methodNumber, string alias)
        {
            if(IsMethodDefined(alias))
                throw new ArgumentException("Метод (" + alias + ") уже определен");

            if(!Utils.IsValidIdentifier(alias))
                throw new ArgumentException("Некорректное имя (" + alias + ")");

            _methods.RegisterAlias(methodNumber, alias);
        }

        public void SetVariableAlias(int variableNumber, string alias)
        {
            if (IsVarDefined(alias))
                throw new ArgumentException("Переменная (" + alias + ") уже определена");

            if (!Utils.IsValidIdentifier(alias))
                throw new ArgumentException("Некорректное имя (" + alias + ")");

            _variables.RegisterAlias(variableNumber, alias);
            
        }

        public bool IsMethodDefined(string name)
        {
            return _methods.HasName(name);
        }

        public int GetMethodNumber(string name)
        {
            int idx;
            if (_methods.TryGetIdOfName(name, out idx))
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
                return _methods.Count;
            }
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

            for (int i = 0; i < count; i++)
            {
                var index = scope.DefineMethod(context.GetMethodName(i, NameRetrievalMode.Name));
                var alias = context.GetMethodName(i, NameRetrievalMode.OnlyAlias);
                if (!String.IsNullOrWhiteSpace(alias))
                    scope.SetMethodAlias(index, alias);
            }
        }
    }
}
