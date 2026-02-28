/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Reflection;
using OneScript.Contexts.Converters;

namespace ScriptEngine.Machine.Contexts
{
    /// <summary>
    /// Вспомогательный класс для получения MethodInfo статических методов конвертеров.
    /// Основная валидация — компилятор (через generic-атрибут), данный класс — страховочная рантайм-проверка.
    /// </summary>
    internal static class ConverterMethodHelper
    {
        public static readonly MethodInfo IsSkippedArgumentMethod =
            typeof(ValueAdoptionExtensions).GetMethod(
                nameof(ValueAdoptionExtensions.IsSkippedArgument),
                BindingFlags.Static | BindingFlags.Public);

        public static MethodInfo GetToBslValueMethod(Type converterType)
            => GetStaticMethod(converterType, nameof(IBslValueConverter.ToBslValue));

        public static MethodInfo GetToClrValueMethod(Type converterType)
            => GetStaticMethod(converterType, nameof(IBslValueConverter.ToClrValue));

        private static MethodInfo GetStaticMethod(Type converterType, string methodName)
        {
            var method = converterType.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
            if (method == null)
                throw new InvalidOperationException(
                    $"Тип '{converterType.FullName}' не содержит публичный статический метод '{methodName}', " +
                    $"требуемый контрактом {nameof(IBslValueConverter)}.");
            return method;
        }
    }
}
