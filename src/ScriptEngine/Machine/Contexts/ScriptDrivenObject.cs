/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OneScript.Commons;
using OneScript.Contexts;
using ScriptEngine.Machine.Reflection;

namespace ScriptEngine.Machine.Contexts
{
    public abstract class ScriptDrivenObject : PropertyNameIndexAccessor, IRunnable
    {
        private LoadedModule _module;
        private IVariable[] _state;
        private int VARIABLE_COUNT;
        private int METHOD_COUNT;
        private MethodSignature[] _attachableMethods;
        private readonly Dictionary<string, int> _methodSearchCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _propertySearchCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _allPropertiesSearchCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public LoadedModule Module => _module;

        protected ScriptDrivenObject(LoadedModule module, bool deffered)
        {
            _module = module;
            if (!deffered)
            {
                InitOwnData();
            }
        }

        protected ScriptDrivenObject(LoadedModule module)
        {
            _module = module;
            InitOwnData();
        }

        protected ScriptDrivenObject()
        {
        }

        protected void SetModule(LoadedModule module)
        {
            _module = module;
        }

        public void InitOwnData()
        {
            
            VARIABLE_COUNT = GetOwnVariableCount();
            METHOD_COUNT = GetOwnMethodCount();

            int stateSize = VARIABLE_COUNT + _module.Variables.Count;
            _state = new IVariable[stateSize];
            for (int i = 0; i < stateSize; i++)
            {
                if (i < VARIABLE_COUNT)
                    _state[i] = Variable.CreateContextPropertyReference(this, i, GetOwnPropName(i));
                else
                    _state[i] = Variable.Create(ValueFactory.Create(), _module.Variables[i-VARIABLE_COUNT]);
            }

            ReadExportedSymbols(_module.ExportedMethods, _methodSearchCache);
            ReadExportedSymbols(_module.ExportedProperies, _propertySearchCache);
            ReadVariables(_module.Variables, _allPropertiesSearchCache);

        }

        private void ReadVariables(VariablesFrame vars, Dictionary<string, int> searchCache)
        {
            for (int i = 0; i < vars.Count; i++)
            {
                var variable = vars[i];
                searchCache[variable.Identifier] = variable.Index;
            }
        }

        private void ReadExportedSymbols(ExportedSymbol[] exportedSymbols, Dictionary<string, int> searchCache)
        {
            for (int i = 0; i < exportedSymbols.Length; i++)
            {
                var es = exportedSymbols[i];
                searchCache[es.SymbolicName] = es.Index;
            }
        }

        protected abstract int GetOwnVariableCount();
        protected abstract int GetOwnMethodCount();
        protected abstract void UpdateState();
        
        public bool MethodDefinedInScript(int index)
        {
            return index >= METHOD_COUNT;
        }

        public bool PropDefinedInScript(int index)
        {
            return index >= VARIABLE_COUNT;
        }

        internal int GetMethodDescriptorIndex(int indexInContext)
        {
            return indexInContext - METHOD_COUNT;
        }

        protected virtual void OnInstanceCreation()
        {
            MachineInstance.Current.ExecuteModuleBody(this);
        }
        
        protected virtual Task OnInstanceCreationAsync()
        {
            return MachineInstance.Current.ExecuteModuleBodyAsync(this);
        }

        public void Initialize()
        {
            OnInstanceCreation();
        }
        
        public Task InitializeAsync()
        {
            return OnInstanceCreationAsync();
        }

        protected int GetScriptMethod(string methodName, string alias = null)
        {
            int index = -1;

            for (int i = 0; i < _module.Methods.Length; i++)
            {
                var item = _module.Methods[i];
                if (StringComparer.OrdinalIgnoreCase.Compare(item.Signature.Name, methodName) == 0
                    || (alias != null && StringComparer.OrdinalIgnoreCase.Compare(item.Signature.Name, alias) == 0))
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        protected IValue CallScriptMethod(int methodIndex, IValue[] parameters)
        {
            var returnValue = MachineInstance.Current.ExecuteMethod(this, methodIndex, parameters);

            return returnValue;
        }

        public Action<IValue[]> GetMethodExecutor(string methodName)
        {
            var id = GetScriptMethod(methodName);
            if (id == -1)
                throw RuntimeException.MethodNotFoundException(methodName, SystemType.Name);

            return (args) => CallScriptMethod(id, args);
        }
        
        #region Own Members Call

        protected virtual int FindOwnProperty(string name)
        {
            return -1;
        }

        protected virtual int FindOwnMethod(string name)
        {
            return -1;
        }

        protected virtual bool IsOwnPropReadable(int index)
        {
            return false;
        }

        protected virtual bool IsOwnPropWritable(int index)
        {
            return false;
        }

        protected virtual IValue GetOwnPropValue(int index)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetOwnPropName(int index)
        {
            throw new NotImplementedException();
        }

        protected virtual void SetOwnPropValue(int index, IValue val)
        {
            throw new NotImplementedException();
        }

        protected virtual BslMethodInfo GetOwnMethod(int index)
        {
            throw new NotImplementedException();
        }
        
        protected virtual BslPropertyInfo GetOwnPropertyInfo(int index)
        {
            throw new NotImplementedException();
        }

        protected virtual void CallOwnProcedure(int index, IValue[] arguments)
        {
            throw new NotImplementedException();
        }

        protected virtual IValue CallOwnFunction(int index, IValue[] arguments)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IAttachableContext Members

        public void OnAttach(MachineInstance machine, out IVariable[] variables, out MethodSignature[] methods)
        {
            UpdateState();

            variables = _state;
            methods = AttachMethods();
        }

        private MethodSignature[] AttachMethods()
        {
            if (_attachableMethods != null)
                return _attachableMethods;

            int totalMethods = METHOD_COUNT + _module.Methods.Length;
            _attachableMethods = new MethodSignature[totalMethods];

            var moduleMethods = _module.Methods.Select(x => x.Signature).ToArray();

            for (int i = 0; i < totalMethods; i++)
            {
                if (MethodDefinedInScript(i))
                {
                    _attachableMethods[i] = moduleMethods[i - METHOD_COUNT];
                }
                else
                {
                    _attachableMethods[i] = GetOwnMethod(i).MakeSignature();
                }
            }

            return _attachableMethods;
        }

        #endregion

        #region IRuntimeContextInstance Members

        public override int FindProperty(string name)
        {
            var idx = FindOwnProperty(name);
            if (idx >= 0)
            {
                return idx;
            }
            else
            {
                int index;
                if (_propertySearchCache.TryGetValue(name, out index))
                    return index;
                else
                    throw PropertyAccessException.PropNotFoundException(name);
            }
        }

        public override int FindMethod(string name)
        {
            var idx = FindOwnMethod(name);
            if (idx >= 0)
            {
                return idx;
            }
            else
            {
                int index;
                if (_methodSearchCache.TryGetValue(name, out index))
                    return index;
                else
                    throw RuntimeException.MethodNotFoundException(name, _module.ModuleInfo.ModuleName);
            }
        }

        public override bool IsPropReadable(int propNum)
        {
            if (PropDefinedInScript(propNum))
            {
                return true;
            }
            else
            {
                return IsOwnPropReadable(propNum);
            }
        }

        public override bool IsPropWritable(int propNum)
        {
            if (PropDefinedInScript(propNum))
            {
                return true;
            }
            else
            {
                return IsOwnPropWritable(propNum);
            }
        }

        public override IValue GetPropValue(int propNum)
        {
            if (PropDefinedInScript(propNum))
            {
                 return _state[propNum].Value;
            }
            else
            {
                return GetOwnPropValue(propNum);
            }
            
        }

        public override void SetPropValue(int propNum, IValue newVal)
        {
            if (PropDefinedInScript(propNum))
            {
                _state[propNum].Value = newVal;
            }
            else
            {
                SetOwnPropValue(propNum, newVal);
            }
            
        }

        public override BslPropertyInfo GetPropertyInfo(int propertyNumber)
        {
            if (PropDefinedInScript(propertyNumber))
            {
                var variable = _module.Variables[propertyNumber-VARIABLE_COUNT];
                return BslPropertyBuilder.Create()
                    .Name(variable.Identifier)
                    .IsExported(_propertySearchCache.ContainsKey(variable.Identifier))
                    .SetAnnotations(variable.Annotations.Select(x => x.MakeBslAttribute()))
                    .Build();
            }
            else
            {
                return GetOwnPropertyInfo(propertyNumber);
            }
        }

        public override BslMethodInfo GetMethodInfo(int methodNumber)
        {
            if (MethodDefinedInScript(methodNumber))
            {
                return _module.Methods[methodNumber-METHOD_COUNT].MethodInfo;
            }
            else
            {
                return GetOwnMethod(methodNumber);
            }
        }

        public override void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            if (MethodDefinedInScript(methodNumber))
            {
                MachineInstance.Current.ExecuteMethod(this, methodNumber - METHOD_COUNT, arguments);
            }
            else
            {
                CallOwnProcedure(methodNumber, arguments);
            }
        }

        public override void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            if (MethodDefinedInScript(methodNumber))
            {
                retValue = MachineInstance.Current.ExecuteMethod(this, methodNumber - METHOD_COUNT, arguments);
            }
            else
            {
                retValue = CallOwnFunction(methodNumber, arguments);
            }
            
        }

        public override int GetPropCount()
        {
            return VARIABLE_COUNT + _module.ExportedProperies.Length;
        }
        
        public override int GetMethodsCount()
        {
            return METHOD_COUNT + _module.ExportedMethods.Length;
        }

        public override string GetPropName(int propNum)
        {
            if(PropDefinedInScript(propNum))
            {
                return _module.ExportedProperies[propNum - VARIABLE_COUNT].SymbolicName;
            }
            else
            {
                return GetOwnPropName(propNum);
            }
        }

        #endregion

        public int FindAnyProperty(string name)
        {
            int index;
            if (_allPropertiesSearchCache.TryGetValue(name, out index))
                return index;
            else
                throw PropertyAccessException.PropNotFoundException(name);
        }

        public string[] GetExportedProperties()
        {
            return _module.ExportedProperies.Select(x => x.SymbolicName).ToArray();
        }

        public string[] GetExportedMethods()
        {
            return _module.ExportedMethods.Select(x => x.SymbolicName).ToArray();
        }
    }
}
