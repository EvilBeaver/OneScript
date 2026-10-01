/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Machine;

namespace OneScript.StandardLibrary.NativeApi
{
    /// <summary>
    /// Экземпляр внешней компоненты Native API
    /// </summary>
    class NativeApiComponent : BslObjectValue, IRuntimeContextInstance, IDisposable
    {
        private IntPtr _object;
        private TypeDescriptor _type;
        private readonly NativeApiProxy.OnErrorDelegate _onError;
        private readonly NativeApiProxy.OnEventDelegate _onEvent;
        private readonly NativeApiProxy.OnStatusDelegate _onStatus;

        public event OnComponentEvent OnComponentEvent;

        public event OnComponentError OnComponentError;

        public event OnComponentStatusText OnComponentStatusText;
        
        private enum ErrorCodes
        {
            ADDIN_E_NONE = 1000,
            ADDIN_E_ORDINARY = 1001,
            ADDIN_E_ATTENTION = 1002,
            ADDIN_E_IMPORTANT = 1003,
            ADDIN_E_VERY_IMPORTANT = 1004,
            ADDIN_E_INFO = 1005,
            ADDIN_E_FAIL = 1006,
            ADDIN_E_MSGBOX_ATTENTION = 1007,
            ADDIN_E_MSGBOX_INFO = 1008,
            ADDIN_E_MSGBOX_FAIL = 1009,
        }

        private static string S(IntPtr ptr)
        {
            return NativeApiProxy.Str(ptr);
        }

        private MessageStatusEnum Status(ushort wcode)
        {
            switch ((ErrorCodes)wcode)
            {
                case ErrorCodes.ADDIN_E_NONE:
                    return MessageStatusEnum.WithoutStatus;
                case ErrorCodes.ADDIN_E_ORDINARY:
                    return MessageStatusEnum.Ordinary;
                case ErrorCodes.ADDIN_E_IMPORTANT:
                    return MessageStatusEnum.Important;
                case ErrorCodes.ADDIN_E_INFO:
                    return MessageStatusEnum.Information;
                case ErrorCodes.ADDIN_E_VERY_IMPORTANT:
                case ErrorCodes.ADDIN_E_FAIL:
                    return MessageStatusEnum.VeryImportant;
                case ErrorCodes.ADDIN_E_ATTENTION:
                case ErrorCodes.ADDIN_E_MSGBOX_ATTENTION:
                case ErrorCodes.ADDIN_E_MSGBOX_INFO:
                case ErrorCodes.ADDIN_E_MSGBOX_FAIL:
                    return MessageStatusEnum.Attention;
                default:
                    return MessageStatusEnum.Ordinary;
            }
        }

        public NativeApiComponent(
            object host,
            NativeApiLibrary library,
            TypeDescriptor typeDef,
            string componentName,
            string identifier,
            bool throwOnZero = true)
        {
            if (!NativeApiProxy.IsAvailable)
                throw new RuntimeException("Native API Proxy DLL is not loaded");
                
            _onError = (wcode, source, descr, scode) =>
                OnComponentError?.Invoke(Status(wcode), scode, S(source), S(descr));
            _onEvent = (source, message, data) =>
                OnComponentEvent?.Invoke(S(source), S(message), S(data));
            _onStatus = status =>
                OnComponentStatusText?.Invoke(S(status));

            _object = NativeApiProxy.GetClassObject(library.Module, componentName, _onError, _onEvent, _onStatus);
            if (_object == IntPtr.Zero)
            {
                if (throwOnZero)
                    throw new RuntimeException($"Не удалось создать объект `{componentName}` внешней компоненты `{identifier}`");
                return;
            }

            _type = typeDef;
        }

        internal bool IsCreated => _object != IntPtr.Zero;

        internal string GetExtensionName()
        {
            var name = string.Empty;
            NativeApiProxy.GetExtensionName(_object, n => name = NativeApiProxy.Str(n));
            return name;
        }
        
        // ReSharper disable once ConvertToAutoProperty
        public override TypeDescriptor SystemType => _type;

        public bool IsIndexed => true;

        public bool DynamicMethodSignatures => false;

        public IValue GetIndexedValue(IValue index)
        {
            if (index.SystemType != BasicTypes.String)
                throw RuntimeException.InvalidArgumentType();
            
            var propNum = GetPropertyNumber(index.ToString());
            return GetPropValue(propNum);
        }

        public void SetIndexedValue(IValue index, IValue value)
        {
            if (index.SystemType != BasicTypes.String)
                throw RuntimeException.InvalidArgumentType();
            
            var propNum = GetPropertyNumber(index.ToString());
            SetPropValue(propNum, value);
        }

        public int GetPropertyNumber(string name)
        {
            var propNumber = NativeApiProxy.FindProp(_object, name);
            if (propNumber < 0)
                throw PropertyAccessException.PropNotFoundException(name);
            return propNumber;
        }

        public bool IsPropReadable(int propNum)
        {
            return NativeApiProxy.IsPropReadable(_object, propNum);
        }

        public bool IsPropWritable(int propNum)
        {
            return NativeApiProxy.IsPropWritable(_object, propNum);
        }

        public int GetPropCount()
        {
            return NativeApiProxy.GetNProps(_object);
        }

        public string GetPropName(int propNum)
        {
            var name = string.Empty;
            NativeApiProxy.GetPropName(_object, propNum, 0,
                n => name = NativeApiProxy.Str(n)
            );
            return name;
        }

        public IValue GetPropValue(int propNum)
        {
            IValue result = ValueFactory.Create();
            NativeApiProxy.GetPropVal(_object, propNum,
                variant => result = new NativeApiVariant(variant).GetValue()
            );
            return result;
        }

        public void SetPropValue(int propNum, IValue value)
        {
            using (var buffer = new NativeApiVariantArray(1))
            {
                buffer[0].Assign(value);
                NativeApiProxy.SetPropVal(_object, propNum, buffer.Ptr);
            }
        }

        public int GetMethodsCount()
        {
            return NativeApiProxy.GetNMethods(_object);
        }

        public int GetMethodNumber(string name)
        {
            var methodNumber = NativeApiProxy.FindMethod(_object, name);
            if (methodNumber < 0)
                throw RuntimeException.MethodNotFoundException(name);
            return methodNumber;
        }

        public BslMethodInfo GetMethodInfo(int methodNumber)
        {
            var method = BslMethodBuilder.Create();
            if (methodNumber < 0)
                throw new RuntimeException("Метод не найден");
            
            NativeApiProxy.GetMethodName(_object, methodNumber, 0,
                str => method.Name(NativeApiProxy.Str(str))
            );
            
            NativeApiProxy.GetMethodName(_object, methodNumber, 1,
                str => method.Alias(NativeApiProxy.Str(str))
            );
            
            var paramCount = NativeApiProxy.GetNParams(_object, methodNumber);
            for (int i = 0; i < paramCount; i++)
            {
                var parameter = method.NewParameter()
                    .Name($"p{i}")
                    .ByValue(false);
                
                if (NativeApiProxy.HasParamDefValue(_object, methodNumber, i))
                {
                    parameter.DefaultValue(BslSkippedParameterValue.Instance);
                    // кажется что значение не нужно в данном кейсе использования
                    // если что - раскомментировать
                    // NativeApiProxy.GetParamDefValue(_object, methodNumber, i, variant =>
                    // {
                    //     var value = (BslPrimitiveValue)new NativeApiVariant(variant).GetValue();
                    //     parameter.DefaultValue(value);
                    // });
                }
            }

            method.ReturnType(NativeApiProxy.HasRetVal(_object, methodNumber) ? typeof(BslValue) : typeof(void));
            method.IsExported(true);

            return method.Build();
        }
        
        public BslPropertyInfo GetPropertyInfo(int propertyNumber)
        {
            var propName = GetPropName(propertyNumber);
            var isReadable = IsPropReadable(propertyNumber);
            var isWritable = IsPropWritable(propertyNumber);

            return BslPropertyBuilder.Create()
                .Name(propName)
                .CanRead(isReadable)
                .CanWrite(isWritable)
                .Build();
        }

        private void SetDefValues(int methodNumber, int paramCount, IValue[] arguments)
        {
            for (int i = 0; i < paramCount; i++)
                if (arguments[i] == null)
                    NativeApiProxy.GetParamDefValue(_object, methodNumber, i,
                        variant => arguments[i] = new NativeApiVariant(variant).GetValue()
                    );
        }

        private static void RemapOutputParameters(
            int paramCount,
            IValue[] arguments,
            IValue[] initialValues,
            NativeApiVariantArray parameters)
        {
            for (int i = 0; i < paramCount; i++)
            {
                if (initialValues[i] == null ||
                    arguments[i] is not IVariable variable)
                    continue;

                var valueAfterCall = parameters[i].GetValue();
                if (!initialValues[i].StrictEquals(valueAfterCall))
                    variable.Value = valueAfterCall;
            }
        }

        public void CallAsProcedure(int methodNumber, IValue[] arguments, IBslProcess process)
        {
            int paramCount = NativeApiProxy.GetNParams(_object, methodNumber);
            using (var parameters = new NativeApiVariantArray(paramCount))
            {
                SetDefValues(methodNumber, paramCount, arguments);

                var initialValues = new IValue[paramCount];
                for (int i = 0; i < paramCount; i++)
                {
                    if (arguments[i] is IVariable reference)
                        initialValues[i] = reference.Value;
                    parameters[i].Assign(arguments[i]);
                }

                if (NativeApiProxy.CallAsProc(_object, methodNumber, parameters.Ptr))
                    RemapOutputParameters(paramCount, arguments, initialValues, parameters);
            }
        }

        public void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue, IBslProcess process)
        {
            var result = ValueFactory.Create();
            int paramCount = NativeApiProxy.GetNParams(_object, methodNumber);
            using (var parameters = new NativeApiVariantArray(paramCount))
            {
                SetDefValues(methodNumber, paramCount, arguments);

                var initialValues = new IValue[paramCount];
                for (int i = 0; i < paramCount; i++)
                {
                    if (arguments[i] is IVariable reference)
                        initialValues[i] = reference.Value;
                    parameters[i].Assign(arguments[i]);
                }

                if (NativeApiProxy.CallAsFunc(_object, methodNumber, parameters.Ptr,
                    res => result = new NativeApiVariant(res).GetValue()))
                {
                    RemapOutputParameters(paramCount, arguments, initialValues, parameters);
                }
            }
            retValue = result;
        }

        public void Dispose()
        {
            if (_object == IntPtr.Zero)
                return;

            NativeApiProxy.DestroyObject(_object);
            _object = IntPtr.Zero;
        }
    }
}
