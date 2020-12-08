/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ScriptEngine.Compiler;

namespace ScriptEngine.Machine.Contexts
{
    public class UserScriptContextInstance : ThisAwareScriptedObjectBase, IDebugPresentationAcceptor
    {
        readonly LoadedModule _module;
        Dictionary<string, int> _ownPropertyIndexes;
        List<IValue> _ownProperties;

        private Func<string> _asStringOverride;

        private const string RAISEEVENT_RU = "ВызватьСобытие";
        private const string RAISEEVENT_EN = "RaiseEvent";
        private const int RAIZEEVENT_INDEX = 0;
        
        public IValue[] ConstructorParams { get; private set; }
        
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
            ActivateAsStringOverride();

            base.OnInstanceCreation();
            var methId = GetScriptMethod("ПриСозданииОбъекта", "OnObjectCreate");
            int constructorParamsCount = ConstructorParams.Count();

            if (methId > -1)
            {
                var procInfo = GetMethodInfo(GetOwnMethodCount()+methId);

                int procParamsCount = procInfo.Params.Count();

                int reqParamsCount = procInfo.Params.Count(x => !x.HasDefaultValue);

                if (constructorParamsCount < reqParamsCount || constructorParamsCount > procParamsCount)
                    throw new RuntimeException("Параметры конструктора: "
                        + "необходимых параметров: " + Math.Min(procParamsCount, reqParamsCount).ToString()
                        + ", передано параметров " + constructorParamsCount.ToString()
                        );
                else if (procInfo.Params.Skip(constructorParamsCount).Any(param => !param.HasDefaultValue))
                    throw RuntimeException.TooFewArgumentsPassed();

                CallScriptMethod(methId, ConstructorParams);
            }
            else
            {
                if (constructorParamsCount > 0)
                {
                    throw new RuntimeException("Конструктор не определен, но переданы параметры конструктора.");
                }
            }
        }

        private void ActivateAsStringOverride()
        {
            var methId = GetScriptMethod("ОбработкаПолученияПредставления", "PresentationGetProcessing");
            if (methId == -1)
                _asStringOverride = base.AsString;
            else
            {
                var signature = GetMethodInfo(methId);
                if (signature.ArgCount != 2)
                    throw new RuntimeException("Обработчик получения представления должен иметь 2 параметра");

                _asStringOverride = () => GetOverridenPresentation(methId);
            }
        }

        private string GetOverridenPresentation(int methId)
        {
            var standard = ValueFactory.Create(true);
            var strValue = ValueFactory.Create();

            var arguments = new IValue[2]
            {
                Variable.Create(strValue, "string"),
                Variable.Create(standard, "standardProcessing")
            };

            CallScriptMethod(methId, arguments);

            if (arguments[1].AsBoolean() == true)
                return base.AsString();

            return arguments[0].AsString();
        }

        public void AddProperty(string name, string alias, IValue value)
        {
            if(_ownProperties == null)
            {
                _ownProperties = new List<IValue>();
                _ownPropertyIndexes = new Dictionary<string, int>();
            }

            var newIndex = _ownProperties.Count + base.GetOwnVariableCount();
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
            return 1;
        }

        protected override int FindOwnMethod(string name)
        {
            if (string.Equals(RAISEEVENT_EN, name, StringComparison.OrdinalIgnoreCase)
                || string.Equals(RAISEEVENT_RU, name, StringComparison.OrdinalIgnoreCase))
            {
                return RAIZEEVENT_INDEX;
            }

            return base.FindOwnMethod(name);
        }

        protected override int FindOwnProperty(string name)
        {
            if (_ownPropertyIndexes != default && _ownPropertyIndexes.TryGetValue(name, out var index))
            {
                return index;
            }

            return base.FindOwnProperty(name);
        }

        protected override MethodInfo GetOwnMethod(int index)
        {
            Debug.Assert(index == RAIZEEVENT_INDEX);

            return GetOwnMethodsDefinition()[RAIZEEVENT_INDEX];
        }

        public static void PrepareCompilation(ICompilerService compiler)
        {
            RegisterSymbols(compiler);
            GetOwnMethodsDefinition().ForEach(x => compiler.DefineMethod(x));
        }
        
        private static MethodInfo[] GetOwnMethodsDefinition()
        {
            return new []{
                new MethodInfo {
                    Name = RAISEEVENT_RU,
                    Alias = RAISEEVENT_EN,
                    IsFunction = false,
                    IsExport = false,
                    Annotations = new AnnotationDefinition[0],
                    Params = new[]
                    {
                        new ParameterDefinition
                        {
                            Name = "eventName",
                            HasDefaultValue = false
                        },
                        new ParameterDefinition
                        {
                            Name = "eventArgs",
                            HasDefaultValue = true,
                            DefaultValueIndex = ParameterDefinition.UNDEFINED_VALUE_INDEX
                        }
                    }
                }
            };
        }

        protected override void CallOwnProcedure(int index, IValue[] arguments)
        {
            Debug.Assert(index == RAIZEEVENT_INDEX);

            var eventName = arguments[0].AsString();
            IValue[] eventArgs = null;
            if (arguments.Length > 1)
            {
                if (arguments[1].AsObject() is IEnumerable<IValue> argsArray)
                {
                    eventArgs = argsArray.ToArray();
                }
            }

            if (eventArgs == null)
                eventArgs = new IValue[0];
            
            MachineInstance.Current.EventProcessor?.HandleEvent(this, eventName, eventArgs);
        }

        protected override int GetOwnVariableCount()
        {
            return base.GetOwnVariableCount() + (_ownProperties?.Count ?? 0);
        }

        protected override void UpdateState()
        {
        }

        protected override bool IsOwnPropReadable(int index)
        {
            if (_ownProperties == null)
                return base.IsOwnPropReadable(index);

            var baseProps = base.GetOwnVariableCount(); 
            if (index >= baseProps)
                return true;
            else
                return base.IsOwnPropReadable(index);
        }

        protected override bool IsOwnPropWritable(int index)
        {
            if (_ownProperties == null)
                return base.IsOwnPropWritable(index);

            return false;
        }

        protected override IValue GetOwnPropValue(int index)
        {
            var baseProps = base.GetOwnVariableCount(); 
            if (index >= baseProps)
                return _ownProperties[index-baseProps];
            else
                return base.GetOwnPropValue(index);
        }
        
        protected override string GetOwnPropName(int index)
        {
            if (_ownProperties == null || index < base.GetOwnVariableCount())
                return base.GetOwnPropName(index);
            
            return _ownPropertyIndexes.First(x => x.Value == index).Key;
        }
        
        public override int GetMethodsCount()
        {
            return GetOwnMethodCount() + _module.Methods.Length;
        }

        public override string AsString()
        {
            return _asStringOverride();
        }

        void IDebugPresentationAcceptor.Accept(IDebugValueVisitor visitor)
        {
            var propVariables = this.GetProperties()
                .Where(x => x.Identifier != ThisAwareScriptedObjectBase.THISOBJ_RU)
                .Select(x => Variable.Create(GetPropValue(x.Index), x.Identifier));
            
            visitor.ShowCustom(propVariables.ToList());
        }
    }
}
