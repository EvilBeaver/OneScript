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
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.Localization;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Types;

namespace ScriptEngine.Machine.Contexts
{
    [ContextClass("Сценарий", "Script")]
    public class UserScriptContextInstance : ThisAwareScriptedObjectBase, IDebugPresentationAcceptor
    {
        public static readonly BilingualString OnInstanceCreationTerms =
            new BilingualString("ПриСозданииОбъекта", "OnObjectCreate");
        
        public static readonly BilingualString PresentationGetProcessingTerms =
            new BilingualString("ОбработкаПолученияПредставления", "PresentationGetProcessing");
        
        public static readonly BilingualString RaiseEventTerms =
            new BilingualString("ВызватьСобытие", "RaiseEvent");

        private const int RAIZEEVENT_INDEX = 0;
        
        Dictionary<string, int> _ownPropertyIndexes;
        List<IValue> _ownProperties;

        private Func<IBslProcess, string> _asStringOverride;

        
        public UserScriptContextInstance(IExecutableModule module, bool deferred = false) : base(module, deferred)
        {
            ConstructorParams = Array.Empty<IValue>();
            DefineType(GetType().GetTypeFromClassMarkup());
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
        
        private IValue[] ConstructorParams { get; }

        protected override void OnInstanceCreation(IBslProcess process)
        {
            ActivateAsStringOverride();

            base.OnInstanceCreation(process);
            var methId = GetScriptMethod(OnInstanceCreationTerms.Russian, OnInstanceCreationTerms.English);
            int constructorParamsCount = ConstructorParams.Length;

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

                if (constructorParamsCount < procParamsCount)
                {
                    var ctorParameters = new IValue[procParamsCount];
                    ConstructorParams.CopyTo(ctorParameters, 0);
                    for (int i = constructorParamsCount; i < procParamsCount; i++)
                    {
                        ctorParameters[i] = (IValue)parameters[i].DefaultValue;
                    }
                    CallScriptMethod(methId, ctorParameters, process);
                }
                else
                    CallScriptMethod(methId, ConstructorParams, process);
            }
            else
            {
                if (constructorParamsCount > 0)
                {
                    throw new RuntimeException("Конструктор не определен, но переданы параметры конструктора.");
                }
            }
        }
        
        public override string ToString(IBslProcess process)
        {
            return _asStringOverride(process);
        }

        private void ActivateAsStringOverride()
        {
            var methId = GetScriptMethod(PresentationGetProcessingTerms.Russian, PresentationGetProcessingTerms.English);
            if (methId == -1)
                _asStringOverride = base.ToString;
            else
            {
                var signature = GetMethodInfo(GetOwnMethodCount()+methId);
                if (signature.GetParameters().Length != 2)
                    throw new RuntimeException("Обработчик получения представления должен иметь 2 параметра");

                _asStringOverride = (p) => GetOverridenPresentation(methId, p);
            }
        }

        private string GetOverridenPresentation(int methId, IBslProcess process)
        {
            var standard = ValueFactory.Create(true);
            var strValue = ValueFactory.Create();

            var arguments = new IValue[2]
            {
                Variable.Create(strValue, "string"),
                Variable.Create(standard, "standardProcessing")
            };

            CallScriptMethod(methId, arguments, process);

            if (arguments[1].AsBoolean() == true)
                return base.ToString(process);

            if (arguments[0].SystemType != BasicTypes.String && arguments[0].SystemType != BasicTypes.Undefined)
            {
                throw new RuntimeException(new BilingualString(
                    $"Полученное представление имеет тип {arguments[0].SystemType}. Ожидается тип Строка",
                    $"Returned presentation has type {arguments[0].SystemType}. Expected type is String"));
            }

            return arguments[0].ToString();
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
            return RaiseEventTerms.HasName(name) ? RAIZEEVENT_INDEX : base.FindOwnMethod(name);
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

        [SymbolsProvider]
        private static void PrepareCompilation(TypeSymbolsProviderFactory providerFactory, SymbolScope scope)
        {
            var baseSymbols = providerFactory.Get<ThisAwareScriptedObjectBase>();
            baseSymbols.FillSymbols(scope);
            GetOwnMethodsDefinition().ForEach(x => scope.DefineMethod(x.ToSymbol()));
        }
        
        private static BslMethodInfo[] GetOwnMethodsDefinition()
        {
            var methodBuilder = BslMethodBuilder.Create();
            methodBuilder.SetNames(RaiseEventTerms.Russian, RaiseEventTerms.English)
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

        protected override void CallOwnProcedure(int index, IValue[] arguments, IBslProcess process)
        {
            Debug.Assert(index == RAIZEEVENT_INDEX);
            var eventProcessor = process.Services.TryResolve<IEventProcessor>();
            if (eventProcessor == default)
                return;

            var eventName = arguments[0].ExplicitString();
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
            
            eventProcessor.HandleEvent(this, eventName, eventArgs, process);
        }

        protected override int GetOwnVariableCount()
        {
            return base.GetOwnVariableCount() + (_ownProperties?.Count ?? 0);
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

        void IDebugPresentationAcceptor.Accept(IDebugValueVisitor visitor)
        {
            var instanceProps = this.GetProperties()
                .OfType<BslScriptPropertyInfo>()
                .Where(p => p.DispatchId != THISOBJ_VARIABLE_INDEX)
                .OrderBy(x => x.DispatchId)
                .ToDictionary(x => x.Name, x => x.DispatchId);

            var instanceFields = Module
                .Fields
                .OfType<BslScriptFieldInfo>()
                .OrderBy(x => x.DispatchId)
                .Where(x => !instanceProps.ContainsKey(x.Name))
                .ToDictionary(x => $"${x.Name}", x => x.DispatchId);

            var props = instanceProps
                .Concat(instanceFields)
                .Select(x => 
                    Variable.Create(GetPropValue(x.Value), x.Key))
                .ToList();

            visitor.ShowCustom(props);
        }
    }
}
