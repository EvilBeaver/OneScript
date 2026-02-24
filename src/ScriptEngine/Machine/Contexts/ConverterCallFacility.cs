/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Reflection;
using OneScript.Contexts.Converters;
using OneScript.Execution;
using OneScript.Values;

namespace ScriptEngine.Machine.Contexts
{
    internal static class ConverterCallFacility
    {
        /// <summary>
        /// Вызов конвертера параметра при вызове
        /// </summary>
        /// <param name="value">bsl-значение</param>
        /// <param name="defaultValue">значение по умолчанию</param>
        /// <param name="converterType">тип конвертера</param>
        /// <param name="targetType">целевой тип</param>
        /// <param name="process">bsl-процесс</param>
        public static object ConvertParam(IValue value, object defaultValue, Type converterType, Type targetType,
            IBslProcess process)
        {
            var factory = process.Services.Resolve<IValueConverterFactory>();
            var converter = factory.CreateConverter(converterType);

            var converted = converter.ToClrValue((BslValue)value, DefaultConverter.Instance, process);
            return converted ?? defaultValue;
        }

        public static IValue ConvertReturnValue(object objParam, Type converterType, Type targetType)
        {
            throw new NotImplementedException();
        }

        public static MethodInfo ConvertParamMethod { get; } = typeof(ConverterCallFacility)
            .GetMethod(nameof(ConvertParam), BindingFlags.Static | BindingFlags.Public);

        public static MethodInfo ConvertRetValueMethod { get; } = typeof(ConverterCallFacility)
            .GetMethod(nameof(ConvertRetValueMethod), BindingFlags.Static | BindingFlags.Public);

        private class DefaultConverter : IBslValueConverter
        {
            public static readonly IBslValueConverter Instance = new DefaultConverter();

            public BslValue ToBslValue(object value, IBslValueConverter defaultConverter, IBslProcess process)
            {
                return (BslValue)ContextValuesMarshaller.ConvertReturnValue(value, value.GetType());
            }

            public object ToClrValue(BslValue value, IBslValueConverter defaultConverter, IBslProcess process)
            {
                return ContextValuesMarshaller.ConvertParam(value, value.GetType(), process);
            }
        }
    }
}