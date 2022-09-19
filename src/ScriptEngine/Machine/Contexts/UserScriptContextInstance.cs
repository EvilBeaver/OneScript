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
using OneScript.Commons;
using OneScript.Compilation;
using OneScript.Compilation.Binding;
using OneScript.Contexts;
using OneScript.Execution;
using OneScript.Types;
using OneScript.Values;

namespace ScriptEngine.Machine.Contexts
{
    [ContextClass("Сценарий", "Script")]
    public class UserScriptContextInstance : ThisAwareScriptedObjectBase, IDebugPresentationAcceptor
    {
        Dictionary<string, int> _ownPropertyIndexes;
        List<IValue> _ownProperties;

        private Func<string> _asStringOverride;

        private const string RAISEEVENT_RU = "ВызватьСобытие";
        private const string RAISEEVENT_EN = "RaiseEvent";
        private const int RAIZEEVENT_INDEX = 0;
        
        public IValue[] ConstructorParams { get; private set; }
        
        public UserScriptContextInstance(IExecutableModule module, bool deferred = false) : base(module, deferred)
        {
            ConstructorParams = Array.Empty<IValue>();
        }

        public UserScriptContextInstance(IExecutableModule module, TypeDescriptor asObjectOfType, IValue[] args = null)
            : base(module, true)
        {
            DefineType(asObjectOfType);

            ConstructorParams = args;
            if (args == null)
            {
                ConstructorParams = Array.Empty<IValue>();
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

                var parameters = procInfo.GetParameters();
                int procParamsCount = parameters.Length;
                int reqParamsCount = parameters.Count(x => !x.HasDefaultValue);

                if (constructorParamsCount < reqParamsCount || constructorParamsCount > procParamsCount)
                    throw new RuntimeException("Параметры конструктора: "
                        + "необходимых параметров: " + Math.Min(procParamsCount, reqParamsCount).ToString()
                        + ", передано параметров " + constructorParamsCount.ToString()
                        );
                else if (parameters.Skip(constructorParamsCount).Any(param => !param.HasDefaultValue))
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
                _asStringOverride = base.ConvertToString;
            else
            {
                var signature = GetMethodInfo(methId);
                if (signature.GetParameters().Length != 2)
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
                return base.ConvertToString();

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

        protected override BslMethodInfo GetOwnMethod(int index)
        {
            Debug.Assert(index == RAIZEEVENT_INDEX);

            return GetOwnMethodsDefinition()[RAIZEEVENT_INDEX];
        }

        protected override BslPropertyInfo GetOwnPropertyInfo(int index)
        {
            if (index == THISOBJ_VARIABLE_INDEX)
                return base.GetOwnPropertyInfo(index);
            
            var names = _ownPropertyIndexes.Where(x => x.Value == index)
                .Select(x => x.Key)
                .ToArray();
            
            Debug.Assert(names.Length > 0 && names.Length <= 2);
            
            var builder = BslPropertyBuilder.Create()
                .Name(names[0]);
            if (names.Length == 2)
            {
                builder.Alias(names[1]);
            }

            builder.SetDispatchingIndex(index);

            return builder.Build();
        }

        public static void PrepareCompilation(ICompilerFrontend compiler)
        {
            RegisterSymbols(compiler);
            GetOwnMethodsDefinition().ForEach(x => compiler.DefineMethod(x));
        }
        
        public static void PrepareCompilation(SymbolTable symbols)
        {
            RegisterSymbols(symbols);
            GetOwnMethodsDefinition().ForEach(x => symbols.DefineMethod(x.ToSymbol()));
        }
        
        private static BslMethodInfo[] GetOwnMethodsDefinition()
        {
            var methodBuilder = BslMethodBuilder.Create();
            methodBuilder.SetNames(RAISEEVENT_RU, RAISEEVENT_EN)
                .DeclaringType(typeof(UserScriptContextInstance));

            methodBuilder.NewParameter()
                .Name("eventName")
                .ParameterType(typeof(string));

            methodBuilder.NewParameter()
                .Name("eventArgs")
                .ParameterType(typeof(BslValue[]))
                .DefaultValue(BslSkippedParameterValue.Instance);

            return new BslMethodInfo[]{methodBuilder.Build()};
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
            return GetOwnMethodCount() + Module.Methods.Count;
        }

        protected override string ConvertToString()
        {
            return _asStringOverride();
        }

        void IDebugPresentationAcceptor.Accept(IDebugValueVisitor visitor)
        {
            var thisId = GetPropertyNumber(THISOBJ_RU);
            var total = GetPropCount();
            var props = new List<IVariable>(total);
            for (int i = 0; i < total; i++)
            {
                if (i != thisId)
                {
                    props.Add(Variable.Create(GetPropValue(i), GetPropName(i)));
                }
            }
            
            visitor.ShowCustom(props);
        }
    }
}
