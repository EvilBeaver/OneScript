/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.StandardLibrary.Binary;
using OneScript.StandardLibrary.TypeDescriptions;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Xml
{
    [GlobalContext(Category="Функции работы с XML")]
    public class XmlGlobalFunctions : GlobalContextBase<XmlGlobalFunctions>
    {
        private static readonly Dictionary<Type, EnumerationContext> _allowedEnums 
            = new Dictionary<Type, EnumerationContext>();
        
        private XmlGlobalFunctions(IGlobalsManager mgr)
        {
            lock (_allowedEnums)
            {
                _allowedEnums.Clear();
                foreach (var e in new[] {
                             (typeof(ClrEnumValueWrapper<AllowedSignEnum>), typeof(AllowedSignEnum)),
                             (typeof(ClrEnumValueWrapper<AllowedLengthEnum>), typeof(AllowedLengthEnum)),
                             (typeof(ClrEnumValueWrapper<DateFractionsEnum>), typeof(DateFractionsEnum))
                         })
                {
                    _allowedEnums.Add(e.Item1, (EnumerationContext)mgr.GetInstance(e.Item2));
                }
            }
        }

        /// <summary>
        /// Получает XML представление значения для помещения в текст элемента или значение атрибута XML.
        /// </summary>
        /// <param name="value">
        /// Значение. Допустимые типы: Булево, Число, Строка, Дата, УникальныйИдентификатор, ДвоичныеДанные,
        /// Неопределено, Null, а также значения перечислений ДопустимыйЗнак, ДопустимаяДлина, ЧастиДаты
        /// </param>
        /// <returns>
        /// Строковое представление значения. Для двоичных данных - строка в формате Вase64.
        /// При недопустимом типе значения выбрасывается исключение
        /// </returns>
        ///
        [ContextMethod("XMLСтрока", "XMLString")]
        public string XMLString(IValue value)
        {
            if (value.SystemType == BasicTypes.String)
                return value.AsString();
            else if (value.SystemType == BasicTypes.Undefined || value.SystemType == BasicTypes.Null)
                return "";
            else if(value.SystemType == BasicTypes.Boolean)
                return XmlConvert.ToString(value.AsBoolean());
            else if(value.SystemType == BasicTypes.Date)
                return XmlConvert.ToString(value.AsDate(), XmlDateTimeSerializationMode.Unspecified);
            else if(value.SystemType == BasicTypes.Number)
                return XmlConvert.ToString(value.AsNumber());
            else
            {
                var rawValue = value.GetRawValue();
                if(rawValue is BinaryDataContext bdc)
                {
                    return Convert.ToBase64String(bdc.Buffer, Base64FormattingOptions.InsertLineBreaks);
                }
                if(rawValue is GuidWrapper guid)
                {
                    return guid.AsString();
                }
                else if (_allowedEnums.ContainsKey(rawValue.GetType()))
                {
                    return rawValue.AsString();
                }
            }

            throw RuntimeException.InvalidArgumentValue();
        }

        /// <summary>
        /// Выполняет преобразование из строки, полученной из текста элемента или значения атрибута XML,
        /// в значение в соответствии с указанным типом. Действие, обратное действию метода XMLСтрока
        /// </summary>
        /// <param name="givenType">
        /// Тип, значение которого надо получить при преобразовании из строкового представления XML.
        /// Допустимые типы: Булево, Число, Строка, Дата, УникальныйИдентификатор, ДвоичныеДанные,
        /// Неопределено, Null, перечисления ДопустимыйЗнак, ДопустимаяДлина, ЧастиДаты
        /// </param>
        /// <param name="presentation">
        /// Строка, содержащая строковое представление значения соответствующего типа
        /// </param>
        /// <returns>
        /// Значение заданного типа.
        /// При недопустимом типе или неправильном строковом представлении выбрасывается исключение
        /// </returns>
        ///
        [ContextMethod("XMLЗначение", "XMLValue")]
        public IValue XMLValue(IValue givenType, string presentation)
        {
            if (givenType.GetRawValue().SystemType != BasicTypes.Type)
            {
                throw RuntimeException.InvalidNthArgumentType(1);
            }

            var dataType = givenType.GetRawValue() as BslTypeValue;
            Debug.Assert(dataType != null);

            var typeValue = dataType.TypeValue;

            if(typeValue.Equals(BasicTypes.Boolean))
            {
                return ValueFactory.Create(XmlConvert.ToBoolean(presentation));
            }
            else if (typeValue.Equals(BasicTypes.Date))
            {
                return ValueFactory.Create(XmlConvert.ToDateTime(presentation, XmlDateTimeSerializationMode.Unspecified));
            }
            else if (typeValue.Equals(BasicTypes.Number))
            {
                return ValueFactory.Create(XmlConvert.ToDecimal(presentation));
            }
            else if (typeValue.Equals(BasicTypes.String))
            {
                return ValueFactory.Create(presentation);
            }
            else if (typeValue.Equals(BasicTypes.Undefined))
            {
                if (presentation.Trim() == "")
                    return ValueFactory.Create();
                
                throw RuntimeException.InvalidNthArgumentValue(2);
            }
            else if (typeValue.Equals(BasicTypes.Null))
            {
                if (presentation.Trim() == "")
                    return ValueFactory.CreateNullValue();
                
                throw RuntimeException.InvalidNthArgumentValue(2);
            }
            else if (typeValue.ImplementingClass == typeof(GuidWrapper))
            {
                try
                {
                    return new GuidWrapper(presentation);
                }
                catch
                {
                    throw RuntimeException.InvalidNthArgumentValue(2);
                }
            }
            else if (typeValue.ImplementingClass == typeof(BinaryDataContext))
            {
                byte[] bytes = Convert.FromBase64String(presentation);
                return new BinaryDataContext(bytes);
            }
            else if (_allowedEnums.TryGetValue(typeValue.ImplementingClass, out var enumerationContext))
            {
                try
                {
                    return enumerationContext[presentation];
                }
                catch (RuntimeException)
                {
                    throw RuntimeException.InvalidNthArgumentValue(2);
                }
            }
 
            throw RuntimeException.InvalidNthArgumentType(1);
        }
        
        public static IAttachableContext CreateInstance(IGlobalsManager mgr)
        {
            return new XmlGlobalFunctions(mgr);
        }

    }
}
