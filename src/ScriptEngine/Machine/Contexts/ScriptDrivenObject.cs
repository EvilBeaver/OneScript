using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    public abstract class ScriptDrivenObject : PropertyNameIndexAccessor, IAttachableContext
    {
        private LoadedModule _module;
        private MachineInstance _machine;
        private IVariable[] _state;
        private readonly int VARIABLE_COUNT;
        private readonly int METHOD_COUNT;

        public ScriptDrivenObject(LoadedModuleHandle module) : this(module.Module)
        {
        }


        internal ScriptDrivenObject(LoadedModule module)
        {
            _module = module;

            VARIABLE_COUNT = GetVariableCount();
            METHOD_COUNT = GetMethodCount();

            int stateSize = VARIABLE_COUNT + _module.VariableFrameSize;
            _state = new IVariable[stateSize];
            for (int i = 0; i < stateSize; i++)
            {
                if (i < VARIABLE_COUNT)
                    _state[i] = Variable.CreateContextPropertyReference(this, i);
                else
                    _state[i] = Variable.Create(ValueFactory.Create());
            }

        }

        protected abstract int GetVariableCount();
        protected abstract int GetMethodCount();
        protected abstract void UpdateState();
        
        public bool MethodDefinedInScript(int index)
        {
            return index >= METHOD_COUNT;
        }

        public bool PropDefinedInScript(int index)
        {
            return index >= VARIABLE_COUNT;
        }

        public void Initialize(MachineInstance runner)
        {
            _machine = runner;
            _machine.StateConsistentOperation(() =>
            {
                _machine.SetModule(_module);
                _machine.AttachContext(this, true);
                _machine.ExecuteModuleBody();
            });
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

        protected virtual void SetOwnPropValue(int index, IValue val)
        {
            throw new NotImplementedException();
        }

        protected virtual MethodInfo GetOwnMethod(int index)
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

        public void OnAttach(MachineInstance machine, out IVariable[] variables, out MethodInfo[] methods, out IRuntimeContextInstance instance)
        {
            UpdateState();

            variables = _state;
            int totalMethods = METHOD_COUNT + _module.Methods.Length;
            methods = new MethodInfo[totalMethods];

            var moduleMethods = _module.Methods.Select(x => x.Signature).ToArray();

            for (int i = 0; i < totalMethods; i++)
            {
                if (MethodDefinedInScript(i))
                {
                    methods[i] = moduleMethods[i];
                }
                else
                {
                    methods[i] = GetOwnMethod(i);
                }
            }
            
            instance = this;

            _machine = machine;

        }

        #endregion

        #region IReflectableContext Members

        public virtual IEnumerable<VariableInfo> GetProperties()
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<MethodInfo> GetMethods()
        {
            throw new NotImplementedException();
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
                var propsFound = _module.ExportedProperies.Where(x => String.Compare(x.SymbolicName, name, true) == 0)
                    .Select(x => x.Index).ToArray();
                if (propsFound.Length > 0)
                {
                    return propsFound[0];
                }
                else
                    throw RuntimeException.PropNotFoundException(name);
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
                var methFound = _module.ExportedMethods.Where(x => String.Compare(x.SymbolicName, name, true) == 0)
                    .Select(x => x.Index).ToArray();
                if (methFound.Length > 0)
                {
                    return methFound[0];
                }
                else
                    throw RuntimeException.MethodNotFoundException(name);
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

        public override MethodInfo GetMethodInfo(int methodNumber)
        {
            if (MethodDefinedInScript(methodNumber))
            {
                return _module.Methods[methodNumber-METHOD_COUNT].Signature;
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
                _machine.StateConsistentOperation(() =>
                {
                    _machine.AttachContext(this, true);
                    _machine.SetModule(_module);
                    _machine.ExecuteMethod(methodNumber - METHOD_COUNT, arguments);
                });
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
                IValue returnClosure = null;
                _machine.StateConsistentOperation(() =>
                {
                    _machine.AttachContext(this, true);
                    _machine.SetModule(_module);
                    returnClosure = _machine.ExecuteMethod(methodNumber, arguments);
                });

                retValue = returnClosure;
            }
            else
            {
                retValue = CallOwnFunction(methodNumber, arguments);
            }
            
        }

        #endregion

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
