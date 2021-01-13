/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine;
using System;
using System.Runtime.InteropServices;

namespace ScriptEngine.HostedScript.Library.NativeApi
{
    /// <summary>
    /// Экземпляр внешней компоненты Native API
    /// </summary>
    class NativeApiComponent : NativeApiValue, IRuntimeContextInstance, IValue
    {
        private static NativeApiProxy _proxy = null;
        private readonly IHostApplication _host;
        private readonly IntPtr _object;

        public static NativeApiProxy proxy
        {
            get {
                if (_proxy == null)
                    _proxy = new NativeApiProxy();
                return _proxy;
            }
        }

        public override IRuntimeContextInstance AsObject()
        {
            return this;
        }

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

        private String S(IntPtr ptr)
        {
            return NativeApiProxy.Str(ptr);
        }

        private MessageStatusEnum Status(UInt16 wcode)
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
                    return MessageStatusEnum.VeryImportant;
                case ErrorCodes.ADDIN_E_ATTENTION:
                case ErrorCodes.ADDIN_E_MSGBOX_INFO:
                case ErrorCodes.ADDIN_E_MSGBOX_FAIL:
                    return MessageStatusEnum.Attention;
                default:
                    return MessageStatusEnum.Ordinary;
            }
        }

        public NativeApiComponent(IHostApplication host, NativeApiLibrary library, String typeName, String componentName)
        {
            _host = host;
            _object = proxy.GetClassObject(library.Module, componentName,
                (wcode, source, descr, scode) =>
                {
                    _host.Echo("ОШИБКА: " + S(source) + " - " + S(descr), Status(wcode));
                },
                (source, message, data) => {
                    _host.Echo("СОБЫТИЕ: " + S(source) + " - " + S(message) + " - " + S(data));
                },
                (status) => {
                    _host.Echo("СТАТУС: " + S(status));
                }
            );
            DefineType(TypeManager.GetTypeByName(typeName));
        }

        public void Dispose()
        {
            try { proxy.DestroyObject(_object); } catch (Exception) { }
        }

        public bool IsIndexed => false;

        public bool DynamicMethodSignatures => false;

        public IValue GetIndexedValue(IValue index)
        {
            throw new NotImplementedException();
        }

        public void SetIndexedValue(IValue index, IValue val)
        {
            throw new NotImplementedException();
        }

        public int FindProperty(string name)
        {
            var propNumber = proxy.FindProp(_object, name);
            if (propNumber < 0)
                throw RuntimeException.PropNotFoundException(name);
            return propNumber;
        }

        public bool IsPropReadable(int propNum)
        {
            return proxy.IsPropReadable(_object, propNum);
        }

        public bool IsPropWritable(int propNum)
        {
            return proxy.IsPropWritable(_object, propNum);
        }

        public int GetPropCount()
        {
            return proxy.GetNProps(_object);
        }

        public string GetPropName(int propNum)
        {
            var name = String.Empty;
            proxy.GetPropName(_object, propNum, 0,
                n => name = NativeApiProxy.Str(n)
            );
            return name;
        }

        public IValue GetPropValue(int propNum)
        {
            var result = ValueFactory.Create();
            proxy.GetPropVal(_object, propNum,
                variant => result = NativeApiVariant.GetValue(variant)
            );
            return result;
        }

        public void SetPropValue(int propNum, IValue newVal)
        {
            var variant = new NativeApiVariant();
            variant.SetValue(newVal);
            proxy.SetPropVal(_object, propNum, ref variant);
            variant.Clear();
        }

        public int GetMethodsCount()
        {
            return proxy.GetNMethods(_object);
        }

        public int FindMethod(string name)
        {
            var methodNumber = proxy.FindMethod(_object, name);
            if (methodNumber < 0)
                throw RuntimeException.MethodNotFoundException(name);
            return methodNumber;
        }

        public MethodInfo GetMethodInfo(int methodNumber)
        {
            if (methodNumber < 0)
                throw new RuntimeException("Метод не найден");
            var name = String.Empty;
            var alias = String.Empty;
            proxy.GetMethodName(_object, methodNumber, 0,
                str => name = NativeApiProxy.Str(str)
            );
            proxy.GetMethodName(_object, methodNumber, 1,
                str => alias = NativeApiProxy.Str(str)
            );
            var paramCount = proxy.GetNParams(_object, methodNumber);
            var paramArray = new ParameterDefinition[paramCount];
            for (int i = 0; i < paramCount; i++)
                proxy.GetParamDefValue(_object, methodNumber, i, variant =>
                {
                    if (NativeApiVariant.NotEmpty(variant))
                    {
                        paramArray[i].HasDefaultValue = true;
                        paramArray[i].DefaultValueIndex = ParameterDefinition.UNDEFINED_VALUE_INDEX;
                    }
                });

            return new MethodInfo
            {
                Name = name,
                Alias = alias,
                IsFunction = proxy.HasRetVal(_object, methodNumber),
                IsDeprecated = false,
                IsExport = false,
                ThrowOnUseDeprecated = false,
                Params = paramArray,
            };
        }

        private void SetDefValues(int methodNumber, int paramCount, IValue[] arguments)
        {
            for (int i = 0; i < paramCount; i++)
                if (arguments[i] == null)
                    proxy.GetParamDefValue(_object, methodNumber, i,
                        variant => arguments[i] = NativeApiVariant.GetValue(variant)
                    );
        }

        public void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            var paramArray = IntPtr.Zero;
            int paramCount = proxy.GetNParams(_object, methodNumber);
            if (paramCount > 0)
                paramArray = Marshal.AllocHGlobal(NativeApiVariant.Size * paramCount);
            SetDefValues(methodNumber, paramCount, arguments);
            NativeApiVariant.SetValue(paramArray, arguments, paramCount);
            proxy.CallAsProc(_object, methodNumber, paramArray);
            NativeApiVariant.GetValue(arguments, paramArray, paramCount);
            NativeApiVariant.Clear(paramArray, paramCount);
            Marshal.FreeHGlobal(paramArray);
        }

        public void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            var paramArray = IntPtr.Zero;
            int paramCount = proxy.GetNParams(_object, methodNumber);
            if (paramCount > 0)
                paramArray = Marshal.AllocHGlobal(NativeApiVariant.Size * paramCount);
            SetDefValues(methodNumber, paramCount, arguments);
            NativeApiVariant.SetValue(paramArray, arguments, paramCount);
            IValue result = retValue = ValueFactory.Create();
            bool ok = proxy.CallAsFunc(_object, methodNumber, paramArray,
                variant => result = NativeApiVariant.GetValue(variant)
            );
            NativeApiVariant.GetValue(arguments, paramArray, paramCount);
            NativeApiVariant.Clear(paramArray, paramCount);
            Marshal.FreeHGlobal(paramArray);
            if (ok)
                retValue = result;
        }
    }
}
