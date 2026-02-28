/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace OneScript.Contexts.Converters
{
    /// <summary>
    /// Базовый класс атрибута, задающего тип конвертера значения.
    /// Используется для рефлексии в маперах (находит и generic-наследника).
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Property)]
    public abstract class BslValueConverterAttribute : Attribute
    {
        /// <summary>
        /// Тип конвертера, преобразующего значение.
        /// Обязан реализовывать <see cref="IBslValueConverter"/>.
        /// </summary>
        public abstract Type ConverterType { get; }
    }

    /// <summary>
    /// Generic-вариант атрибута конвертера значения.
    /// Обеспечивает компайл-тайм валидацию: T обязан реализовывать <see cref="IBslValueConverter"/>.
    /// </summary>
    /// <typeparam name="T">Тип конвертера</typeparam>
    public sealed class BslValueConverterAttribute<T> : BslValueConverterAttribute
        where T : IBslValueConverter
    {
        public override Type ConverterType => typeof(T);
    }
}
