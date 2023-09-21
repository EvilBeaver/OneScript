/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Xml;

using ScriptEngine.HostedScript.Library.Binary;

namespace ScriptEngine.HostedScript.Library.Xml
{
    [GlobalContext(Category="Функции работы с XML")]
    public class XmlGlobalFunctions : GlobalContextBase<XmlGlobalFunctions>
    {
        private static readonly Dictionary<TypeDescriptor, EnumerationContext> _allowedEnums
            = new Dictionary<TypeDescriptor, EnumerationContext>();

        private static readonly TypeDescriptor _binaryDataTypeDescriptor;
        private static readonly TypeDescriptor _nullTypeDescriptor;
        private static readonly TypeDescriptor _guidTypeDescriptor;

        static XmlGlobalFunctions()
        {
            _binaryDataTypeDescriptor = TypeManager.GetTypeByFrameworkType(typeof(BinaryDataContext));
            _nullTypeDescriptor = TypeManager.GetTypeByName("NULL");
            _guidTypeDescriptor = TypeManager.GetTypeByFrameworkType(typeof(GuidWrapper));

            foreach (var e in new []{typeof(AllowedSignEnum), typeof(AllowedLengthEnum), typeof(DateFractionsEnum) })
            { 
                _allowedEnums.Add(TypeManager.GetTypeByFrameworkType(e), GlobalsManager.GetSimpleEnum(e));
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
            switch(value.DataType)
            {
                case DataType.String:
                    return value.AsString();
                case DataType.Undefined:
                    return "";
                case DataType.Boolean:
                    return XmlConvert.ToString(value.AsBoolean());
                case DataType.Date:
                    return XmlConvert.ToString(value.AsDate(), XmlDateTimeSerializationMode.Unspecified);
                case DataType.Number:
                    return XmlConvert.ToString(value.AsNumber());

                case DataType.Enumeration:
                    if (_allowedEnums.TryGetValue(value.SystemType, out var enumeration))
                    {
                        return value.AsString();
                    }
                    break;

                default:
                    if(value.SystemType.Equals(_binaryDataTypeDescriptor))
                    {
                        var bdc = value.GetRawValue() as BinaryDataContext;
                        System.Diagnostics.Debug.Assert(bdc != null);

                        return Convert.ToBase64String(bdc.Buffer, Base64FormattingOptions.InsertLineBreaks);
                    }
                    else if (value.SystemType.Equals(_guidTypeDescriptor))
                    {
                        return value.AsString();;
                    }
                    else if (value.SystemType.Equals(_nullTypeDescriptor))
                    {
                        return "";
                    }
                    break;
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
            var typeValue = TypeManager.GetTypeDescriptorFor(givenType.GetRawValue());

            if(typeValue.Equals(TypeDescriptor.FromDataType(DataType.Boolean)))
            {
                return ValueFactory.Create(XmlConvert.ToBoolean(presentation));
            }
            else if (typeValue.Equals(TypeDescriptor.FromDataType(DataType.Date)))
            {
                return ValueFactory.Create(XmlConvert.ToDateTime(presentation, XmlDateTimeSerializationMode.Unspecified));
            }
            else if (typeValue.Equals(TypeDescriptor.FromDataType(DataType.Number)))
            {
                return ValueFactory.Create(XmlConvert.ToDecimal(presentation));
            }
            else if (typeValue.Equals(TypeDescriptor.FromDataType(DataType.String)))
            {
                return ValueFactory.Create(presentation);
            }
            else if (typeValue.Equals(TypeDescriptor.FromDataType(DataType.Undefined)))
            {
                if ( presentation.Trim() == "")
                    return ValueFactory.Create();
                else
                {
                    throw RuntimeException.InvalidNthArgumentValue(2);
                }
            }
            else if (typeValue.Equals(_nullTypeDescriptor))
            {
                if (presentation.Trim() == "")
                    return ValueFactory.CreateNullValue();
                else
                {
                    throw RuntimeException.InvalidNthArgumentValue(2);
                }
            }
            else if (typeValue.Equals(_binaryDataTypeDescriptor))
            {
                byte[] bytes = Convert.FromBase64String(presentation);
                return new BinaryDataContext(bytes);
            }
            else if (typeValue.Equals(_guidTypeDescriptor))
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
            else if (_allowedEnums.TryGetValue(typeValue, out var enumerationContext))
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

            throw RuntimeException.InvalidNthArgumentValue(1);
        }
        
        public static IAttachableContext CreateInstance()
        {
            return new XmlGlobalFunctions();
        }

    }
}
