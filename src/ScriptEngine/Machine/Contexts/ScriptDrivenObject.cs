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

namespace ScriptEngine.Machine.Contexts
{
    public abstract class ScriptDrivenObject : PropertyNameIndexAccessor, IRunnable
    {
        private LoadedModule _module;
        private IVariable[] _state;
        private int VARIABLE_COUNT;
        private int METHOD_COUNT;
        private BslMethodInfo[] _attachableMethods;
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

            ClearSearchCaches();

            int stateSize = VARIABLE_COUNT + _module.Fields.Count;
            _state = new IVariable[stateSize];
            for (int i = 0; i < stateSize; i++)
            {
                if (i < VARIABLE_COUNT)
                {
                    var name = GetOwnPropName(i);
                    _state[i] = Variable.CreateContextPropertyReference(this, i, name);
                    _allPropertiesSearchCache.Add(name, i);
                }
                else
                {
                    var name = _module.Fields[i - VARIABLE_COUNT].Name;
                    _state[i] = Variable.Create(ValueFactory.Create(), name);
                    _allPropertiesSearchCache.Add(name, i);
                }
            }

            foreach (var prop in _module.Properties.Cast<BslScriptPropertyInfo>())
            {
                _propertySearchCache.Add(prop.Name, prop.DispatchId);
            }

            foreach (var method in _module.Methods.Cast<BslScriptMethodInfo>())
            {
                if(method.IsPublic)
                    _methodSearchCache.Add(method.Name, method.DispatchId);
            }

        }

        private void ClearSearchCaches()
        {
            _propertySearchCache.Clear();
            _methodSearchCache.Clear();
            _allPropertiesSearchCache.Clear();
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

            for (int i = 0; i < _module.Methods.Count; i++)
            {
                var item = _module.Methods[i];
                if (StringComparer.OrdinalIgnoreCase.Compare(item.Name, methodName) == 0
                    || (alias != null && StringComparer.OrdinalIgnoreCase.Compare(item.Name, alias) == 0))
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

        public void OnAttach(MachineInstance machine, out IVariable[] variables, out BslMethodInfo[] methods)
        {
            UpdateState();

            variables = _state;
            methods = AttachMethods();
        }

        private BslMethodInfo[] AttachMethods()
        {
            if (_attachableMethods != null)
                return _attachableMethods;

            int totalMethods = METHOD_COUNT + _module.Methods.Count;
            _attachableMethods = new BslMethodInfo[totalMethods];

            var moduleMethods = _module.Methods;

            for (int i = 0; i < totalMethods; i++)
            {
                if (MethodDefinedInScript(i))
                {
                    _attachableMethods[i] = moduleMethods[i - METHOD_COUNT];
                }
                else
                {
                    _attachableMethods[i] = GetOwnMethod(i);
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
                    throw RuntimeException.MethodNotFoundException(name, _module.Source.Name);
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
                return _module.Properties[propertyNumber-VARIABLE_COUNT];
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
                return _module.Methods[methodNumber-METHOD_COUNT];
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
            return VARIABLE_COUNT + _module.Properties.Count;
        }
        
        public override int GetMethodsCount()
        {
            return METHOD_COUNT + _module.Methods.Count(x => x.IsPublic);
        }

        public override string GetPropName(int propNum)
        {
            if(PropDefinedInScript(propNum))
            {
                return _module.Properties[propNum - VARIABLE_COUNT].Name;
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
    }
}
