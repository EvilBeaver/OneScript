/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Commons;
using OneScript.Compilation.Binding;
using OneScript.Contexts;
using OneScript.Values;
using ScriptEngine.Compiler;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Libraries
{
    public class ModulesOrderingContext : IAttachableContext
    {
        private readonly SymbolScope _libraryScope;

        private class ModuleToLoad
        {
            public DiscoveryState state;
            public BslValue moduleInstance;
        }
        
        private enum DiscoveryState
        {
            New,
            Discovered,
            Processed
        }

        private readonly IndexedNameValueCollection<ModuleToLoad> _values =
            new IndexedNameValueCollection<ModuleToLoad>();

        private ScriptingEngine _runtimeToInit = null;

        private IVariable[] _attachedState;

        public ModulesOrderingContext(SymbolScope libraryScope)
        {
            _libraryScope = libraryScope;
        }

        public void SetUninitializedInstance(UserAddedScript module, ScriptDrivenObject instance)
        {
            var item = new ModuleToLoad
            {
                state = DiscoveryState.New,
                moduleInstance = instance
            };

            _values.Add(item, module.Symbol);
        }

        public void InitializeModules(ScriptingEngine runtime)
        {
            _runtimeToInit = runtime;
            _attachedState = new IVariable[_values.Count];

            int i = 0;
            foreach (var moduleToLoad in _libraryScope.Variables)
            {
                _attachedState[i] = Variable.CreateContextPropertyReference(this, i, moduleToLoad.Name);
                i++;
            }
            
            foreach (var moduleToLoad in _values)
            {
                // В этот момент блок инициализации модуля
                // через механизм IVariableReference может обратиться к контексту
                // и затребовать какой-то другой модуль
                // Метод получения значения свойства рекурсивно вызовет InitializeSDO для этого модуля
                _runtimeToInit.InitializeSDO((ScriptDrivenObject)moduleToLoad.moduleInstance);
                moduleToLoad.state = DiscoveryState.Processed;
            }

            _runtimeToInit = null;
        }
        
        public bool IsIndexed => false;

        public bool DynamicMethodSignatures => false;

        public IValue GetIndexedValue(IValue index)
        {
            throw new InvalidOperationException();
        }

        public void SetIndexedValue(IValue index, IValue val)
        {
            throw new InvalidOperationException();
        }

        public int GetPropertyNumber(string name)
        {
            var index = _values.IndexOf(name); 
            if (index >= 0)
            {
                return index;
            }
            else
            {
                throw new SymbolNotFoundException(name);
            }
        }

        public bool IsPropReadable(int propNum) => true;

        public bool IsPropWritable(int propNum) => false;

        public IValue GetPropValue(int propNum)
        {
            var item = _values[propNum];
            switch (item.state)
            {
                case DiscoveryState.Processed:
                    return item.moduleInstance;
                case DiscoveryState.Discovered:
                    SystemLogger.Write($"Module {_values.NameOf(propNum)} is not initialized properly due to circular initialization");
                    return item.moduleInstance;
            }

            if (_runtimeToInit == null)
            {
                throw new InvalidOperationException("System error. Runtime is NULL while initializing library");
            }
            
            item.state = DiscoveryState.Discovered;
            _runtimeToInit.InitializeSDO((ScriptDrivenObject)item.moduleInstance);
            item.state = DiscoveryState.Processed;

            return item.moduleInstance;
        }

        public void SetPropValue(int propNum, IValue newVal)
        {
            throw new System.NotImplementedException();
        }

        public int GetPropCount() => _values.Count;

        public string GetPropName(int propNum) => _values.NameOf(propNum);

        public int GetMethodNumber(string name)
        {
            throw new SymbolNotFoundException(name);
        }

        public int GetMethodsCount() => 0;

        public BslMethodInfo GetMethodInfo(int methodNumber)
        {
            throw new System.NotImplementedException();
        }

        public BslPropertyInfo GetPropertyInfo(int propertyNumber)
        {
            return ((IPropertySymbol)_libraryScope.Variables[propertyNumber]).Property;
        }

        public void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            throw new System.NotImplementedException();
        }

        public void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            throw new System.NotImplementedException();
        }

        public void OnAttach(out IVariable[] variables, out BslMethodInfo[] methods)
        {
            throw new NotImplementedException();
        }
    }
}