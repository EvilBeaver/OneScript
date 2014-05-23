using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine.Library;

namespace ScriptEngine.Machine
{
    class UserScriptContextInstance : PropertyNameIndexAccessor, IAttachableContext
    {
        LoadedModule _module;
        MachineInstance _machine;
        IVariable[] _state;

        public UserScriptContextInstance(LoadedModule module)
        {
            Init(module);  
        }

        public UserScriptContextInstance(LoadedModule module, string asObjectOfType)
        {
            DefineType(TypeManager.GetTypeByName(asObjectOfType));
            Init(module);
        }

        private void Init(LoadedModule module)
        {
            _module = module;
            _state = new IVariable[module.VariableFrameSize];
            for (int i = 0; i < module.VariableFrameSize; i++)
            {
                _state[i] = Variable.Create(ValueFactory.Create());
            }
        }

        #region IAttachableContext Members

        public void OnAttach(MachineInstance machine, 
            out IVariable[] variables, 
            out MethodInfo[] methods, 
            out IRuntimeContextInstance instance)
        {
            _machine = machine;
            variables = _state;
            methods = _module.Methods.Select(x => x.Signature).ToArray();
            instance = this;
        }

        #endregion

        public override int FindProperty(string name)
        {
            var propsFound = _module.ExportedProperies.Where(x=>String.Compare(x.SymbolicName, name, true) == 0)
                .Select(x=>x.Index).ToArray();
            if (propsFound.Length > 0)
            {
                return propsFound[0];
            }
            else
                throw RuntimeException.PropNotFoundException(name);
        }

        public override IValue GetPropValue(int propNum)
        {
            return _state[propNum].Value;
        }

        public override void SetPropValue(int propNum, IValue newVal)
        {
            _state[propNum].Value = newVal;
        }

        public override int FindMethod(string name)
        {
            var methFound = _module.ExportedMethods.Where(x=>String.Compare(x.SymbolicName, name, true) == 0)
                .Select(x=>x.Index).ToArray();
            if (methFound.Length > 0)
            {
                return methFound[0];
            }
            else
                throw RuntimeException.MethodNotFoundException(name);
        }

        public override MethodInfo GetMethodInfo(int methodNumber)
        {
            return _module.Methods[methodNumber].Signature;
        }

        public override void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            _machine.StateConsistentOperation(()=>
            {
                _machine.AttachContext(this, true);
                _machine.SetModule(_module);
                _machine.ExecuteMethod(methodNumber, arguments);
            });
        }

        public override void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
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
    }
}
