/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Globalization;
using System.Reflection;
using OneScript.Contexts;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace NUnitTests.Bsl
{
    /// <summary>
    /// Обертка над BslScriptMethodInfo, которая позволяет вызывать метод через Invoke.
    /// Используется для интеграции BSL-методов с NUnit.
    /// </summary>
    public class InvokableBslMethodInfo : BslScriptMethodInfo
    {
        private readonly BslScriptMethodInfo _bslMethod;
        private readonly BslScriptProcess _bslProcess;

        public InvokableBslMethodInfo(BslScriptMethodInfo bslMethod, BslScriptProcess bslProcess)
        {
            _bslMethod = bslMethod ?? throw new ArgumentNullException(nameof(bslMethod));
            _bslProcess = bslProcess ?? throw new ArgumentNullException(nameof(bslProcess));
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (!(obj is UserScriptContextInstance testInstance))
                throw new ArgumentException($"Объект должен быть типа {nameof(UserScriptContextInstance)}", nameof(obj));

            // Конвертируем object[] в IValue[]
            var bslParameters = ConvertParameters(parameters ?? Array.Empty<object>());

            // Получаем номер метода
            var methodNumber = testInstance.GetMethodNumber(_bslMethod.Name);

            // Создаем процесс для вызова
            var process = _bslProcess.Engine.Engine.NewProcess();

            // Вызываем метод
            if (_bslMethod.IsFunction())
            {
                testInstance.CallAsFunction(methodNumber, bslParameters, out IValue result, process);
                // Конвертируем результат обратно в object
                return ConvertResult(result);
            }
            else
            {
                testInstance.CallAsProcedure(methodNumber, bslParameters, process);
                return null;
            }
        }

        private static IValue[] ConvertParameters(object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                return Array.Empty<IValue>();

            var result = new IValue[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                result[i] = ConvertParameter(parameters[i]);
            }
            return result;
        }

        private static IValue ConvertParameter(object param)
        {
            if (param == null)
                return ValueFactory.Create();

            if (param is IValue value)
                return value;

            return ContextValuesMarshaller.ConvertDynamicValue(param);
        }

        private static object ConvertResult(IValue result)
        {
            if (result == null)
                return null;

            if (result.SystemType == BasicTypes.Undefined)
                return null;

            return ContextValuesMarshaller.ConvertToClrObject(result);
        }

        #region MethodInfo delegates to _bslMethod

        public override string Name => _bslMethod.Name;

        public override Type ReturnType => _bslMethod.ReturnType;

        public override Type ReflectedType => _bslMethod.ReflectedType;

        public override MethodAttributes Attributes => _bslMethod.Attributes;

        public override RuntimeMethodHandle MethodHandle
        {
            get
            {
                // BslScriptMethodInfo.MethodHandle выбрасывает NotSupportedException
                // Возвращаем пустой handle, так как для BSL-методов это не используется
                return default(RuntimeMethodHandle);
            }
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return _bslMethod.GetMethodImplementationFlags();
        }

        public override ParameterInfo[] GetParameters()
        {
            return _bslMethod.GetParameters();
        }

        public override MethodInfo GetBaseDefinition()
        {
            return _bslMethod.GetBaseDefinition();
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes => _bslMethod.ReturnTypeCustomAttributes;

        public override Type[] GetGenericArguments()
        {
            return _bslMethod.GetGenericArguments();
        }

        public override MethodInfo GetGenericMethodDefinition()
        {
            return _bslMethod.GetGenericMethodDefinition();
        }

        public override MethodInfo MakeGenericMethod(params Type[] typeArguments)
        {
            var genericMethod = _bslMethod.MakeGenericMethod(typeArguments);
            if (genericMethod is BslScriptMethodInfo bslGenericMethod)
            {
                return new InvokableBslMethodInfo(bslGenericMethod, _bslProcess);
            }
            return genericMethod;
        }

        public override bool IsGenericMethod => _bslMethod.IsGenericMethod;

        public override bool IsGenericMethodDefinition => _bslMethod.IsGenericMethodDefinition;

        public override bool ContainsGenericParameters => _bslMethod.ContainsGenericParameters;

        #endregion
    }
}

