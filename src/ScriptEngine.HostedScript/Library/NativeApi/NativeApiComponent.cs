/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine;
using System;

namespace ScriptEngine.HostedScript.Library.NativeApi
{
    /// <summary>
    /// Экземпляр внешней компоненты Native API
    /// </summary>
    class NativeApiComponent : NativeApiValue, IRuntimeContextInstance, IValue
    {
        private readonly IHostApplication _host;
        private readonly IntPtr _object;

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
            _object = NativeApiProxy.GetClassObject(library.Module, componentName,
                (wcode, source, descr, scode) => _host.Echo("ОШИБКА: " + S(source) + " - " + S(descr), Status(wcode)),
                (source, message, data) => _host.Echo("СОБЫТИЕ: " + S(source) + " - " + S(message) + " - " + S(data)),
                (status) => _host.Echo("СТАТУС: " + S(status))
            );
            DefineType(TypeManager.GetTypeByName(typeName));
        }

        public void Dispose()
        {
            try { NativeApiProxy.DestroyObject(_object); } catch (Exception) { }
        }

        public bool IsIndexed => true;

        public bool DynamicMethodSignatures => false;

        public IValue GetIndexedValue(IValue index)
        {
            var propNum = FindProperty(index.AsString());
            return GetPropValue(propNum);
        }

        public void SetIndexedValue(IValue index, IValue value)
        {
            var propNum = FindProperty(index.AsString());
            SetPropValue(propNum, value);
        }

        public int FindProperty(string name)
        {
            var propNumber = NativeApiProxy.FindProp(_object, name);
            if (propNumber < 0)
                throw RuntimeException.PropNotFoundException(name);
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
            var name = String.Empty;
            NativeApiProxy.GetPropName(_object, propNum, 0,
                n => name = NativeApiProxy.Str(n)
            );
            return name;
        }

        public IValue GetPropValue(int propNum)
        {
            IValue result = ValueFactory.Create();
            NativeApiProxy.GetPropVal(_object, propNum,
                variant => result = NativeApiVariant.Value(variant)
            );
            return result;
        }

        public void SetPropValue(int propNum, IValue value)
        {
            using (var variant = new NativeApiVariant())
            {
                variant.Assign(value);
                NativeApiProxy.SetPropVal(_object, propNum, variant.Ptr);
            };
        }

        public int GetMethodsCount()
        {
            return NativeApiProxy.GetNMethods(_object);
        }

        public int FindMethod(string name)
        {
            var methodNumber = NativeApiProxy.FindMethod(_object, name);
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
            NativeApiProxy.GetMethodName(_object, methodNumber, 0,
                str => name = NativeApiProxy.Str(str)
            );
            NativeApiProxy.GetMethodName(_object, methodNumber, 1,
                str => alias = NativeApiProxy.Str(str)
            );
            var paramCount = NativeApiProxy.GetNParams(_object, methodNumber);
            var paramArray = new ParameterDefinition[paramCount];
            for (int i = 0; i < paramCount; i++)
                if (NativeApiProxy.HasParamDefValue(_object, methodNumber, i))
                {
                    paramArray[i].HasDefaultValue = true;
                    paramArray[i].DefaultValueIndex = ParameterDefinition.UNDEFINED_VALUE_INDEX;
                };

            return new MethodInfo
            {
                Name = name,
                Alias = alias,
                IsFunction = NativeApiProxy.HasRetVal(_object, methodNumber),
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
                    NativeApiProxy.GetParamDefValue(_object, methodNumber, i,
                        variant => arguments[i] = NativeApiVariant.Value(variant)
                    );
        }

        public void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            int paramCount = NativeApiProxy.GetNParams(_object, methodNumber);
            using (var variant = new NativeApiVariant(paramCount))
            {
                SetDefValues(methodNumber, paramCount, arguments);
                for (int i = 0; i < paramCount; i++)
                    variant.Assign(arguments[i], i);

                NativeApiProxy.CallAsProc(_object, methodNumber, variant.Ptr);
            }
        }

        public void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            var result = ValueFactory.Create();
            int paramCount = NativeApiProxy.GetNParams(_object, methodNumber);
            using (var variant = new NativeApiVariant(paramCount))
            {
                SetDefValues(methodNumber, paramCount, arguments);
                for (int i = 0; i < paramCount; i++)
                    variant.Assign(arguments[i], i);

                NativeApiProxy.CallAsFunc(_object, methodNumber, variant.Ptr,
                    res => result = NativeApiVariant.Value(res)
                );
            }
            retValue = result;
        }
    }
}
