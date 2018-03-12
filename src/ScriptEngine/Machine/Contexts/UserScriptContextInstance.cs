/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptEngine.Machine.Contexts
{
    public class UserScriptContextInstance : ScriptDrivenObject
    {
        readonly LoadedModule _module;
        Dictionary<string, int> _ownPropertyIndexes;
        List<IValue> _ownProperties;
        
        public IValue[] ConstructorParams { get; private set; }
        
        static UserScriptContextInstance()
        {
            TypeManager.RegisterType("Сценарий", typeof(UserScriptContextInstance));
        }

        public UserScriptContextInstance(LoadedModule module) : base(module)
        {
            _module = module;
            ConstructorParams = new IValue[0];
        }

        public UserScriptContextInstance(LoadedModule module, string asObjectOfType, IValue[] args = null)
            : base(module, true)
        {
            DefineType(TypeManager.GetTypeByName(asObjectOfType));
            _module = module;

            ConstructorParams = args;
            if (args == null)
            {
                ConstructorParams = new IValue[0];
            }

        }

        protected override void OnInstanceCreation()
        {
            base.OnInstanceCreation();
            var methId = GetScriptMethod("ПриСозданииОбъекта", "OnObjectCreate");
            int constructorParamsCount = ConstructorParams.Count();

            if (methId > -1)
            {
                bool hasParamsError = false;
                var procInfo = GetMethodInfo(methId);

                int procParamsCount = procInfo.Params.Count();

                if (procParamsCount < constructorParamsCount)
                {
                    hasParamsError = true;
                }

                int reqParams = 0;
                foreach (var itm in procInfo.Params)
                {
                    if (!itm.HasDefaultValue) reqParams++;
                }
                if (reqParams > constructorParamsCount)
                {
                    hasParamsError = true;
                }
                if (hasParamsError)
                {
                    throw new RuntimeException("Параметры конструктора: "
                        + "необходимых параметров: " + Math.Min(procParamsCount, reqParams).ToString()
                        + ", передано параметров " + constructorParamsCount.ToString()
                        );
                }

                CallAsProcedure(methId, ConstructorParams);
            }
            else
            {
                if (constructorParamsCount > 0)
                {
                    throw new RuntimeException("Конструктор не определен, но переданы параметры конструктора.");
                }
            }
        }
        
        public void AddProperty(string name, string alias, IValue value)
        {
            if(_ownProperties == null)
            {
                _ownProperties = new List<IValue>();
                _ownPropertyIndexes = new Dictionary<string, int>();
            }

            var newIndex = _ownProperties.Count;
            _ownPropertyIndexes.Add(name, newIndex);
            if (!string.IsNullOrEmpty(alias))
            {
                _ownPropertyIndexes.Add(alias, newIndex);
            }
            _ownProperties.Add(value);

        }

        public void AddProperty(string name, IValue value)
        {
            AddProperty(name, null, value);
        }

        protected override int GetOwnMethodCount()
        {
            return 0;
        }

        protected override int GetOwnVariableCount()
        {
            if (_ownProperties == null)
                return 0;
            else
                return _ownProperties.Count;
        }

        protected override void UpdateState()
        {
        }

        protected override bool IsOwnPropReadable(int index)
        {
            if (_ownProperties == null)
                return false;

            if (index >= 0 && index < _ownProperties.Count)
                return true;
            else
                return false;
        }

        protected override IValue GetOwnPropValue(int index)
        {
            return _ownProperties[index];
        }

        protected override string GetOwnPropName(int index)
        {
            if (_ownProperties == null)
                throw new ArgumentException("Unknown property index");

            return _ownPropertyIndexes.Where(x => x.Value == index).First().Key;
        }
        
        public override int GetMethodsCount()
        {
            return _module.Methods.Length;
        }
        
    }
}
