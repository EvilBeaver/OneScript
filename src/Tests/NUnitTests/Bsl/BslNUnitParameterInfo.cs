/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Reflection;
using NUnit.Framework.Interfaces;

namespace NUnitTests.Bsl
{
    /// <summary>
    /// Реализация IParameterInfo для BSL-параметров, используемая в интеграции с NUnit.
    /// </summary>
    public class BslNUnitParameterInfo : IParameterInfo
    {
        private readonly ParameterInfo _parameter;
        private readonly IMethodInfo _method;

        public BslNUnitParameterInfo(IMethodInfo methodInfo, ParameterInfo parameter)
        {
            _method = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            _parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
        }

        public ParameterInfo ParameterInfo => _parameter;

        public bool IsOptional => _parameter.IsOptional;

        public IMethodInfo Method => _method;

        public Type ParameterType => _parameter.ParameterType;

        public bool IsDefined<T>(bool inherit) where T : class
        {
            return _parameter.IsDefined(typeof(T), inherit);
        }

        public T[] GetCustomAttributes<T>(bool inherit) where T : class
        {
            var attrs = _parameter.GetCustomAttributes(typeof(T), inherit);
            var result = new T[attrs.Length];
            for (int i = 0; i < attrs.Length; i++)
            {
                result[i] = (T)attrs[i];
            }
            return result;
        }

        public ITypeInfo ParameterTypeInfo => new BslNUnitTypeInfo(_parameter.ParameterType);
    }
}

