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
    /// Атрибут задающий тип конвертера значения
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter|AttributeTargets.Method|AttributeTargets.Property)]
    public class BslValueConverterAttribute : Attribute
    {
        /// <summary>
        /// Основной конструктор
        /// </summary>
        /// <param name="converterType">Тип конвертера, преобразующего значение.
        /// Обязан реализовывать <see cref="IBslValueConverter"/></param>
        public BslValueConverterAttribute(Type converterType)
        {
            ConverterType = converterType;
        }
        
        /// <summary>
        /// Тип конвертера, преобразующего значение.
        /// </summary>
        public Type ConverterType { get; }
    }
}