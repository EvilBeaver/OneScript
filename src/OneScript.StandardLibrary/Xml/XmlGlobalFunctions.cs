/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Diagnostics;
using System.Xml;
using OneScript.Core;
using OneScript.StandardLibrary.Binary;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine.Values;
using ScriptEngine.Types;

namespace OneScript.StandardLibrary.Xml
{
    [GlobalContext(Category="Функции работы с XML")]
    public class XmlGlobalFunctions : GlobalContextBase<XmlGlobalFunctions>
    {
        [ContextMethod("XMLСтрока", "XMLString")]
        public string XMLString(IValue value)
        {
            switch(value.DataType)
            {
                case DataType.Undefined:
                    return "";
                case DataType.Boolean:
                    return XmlConvert.ToString(value.AsBoolean());
                case DataType.Date:
                    return XmlConvert.ToString(value.AsDate(), XmlDateTimeSerializationMode.Unspecified);
                case DataType.Number:
                    return XmlConvert.ToString(value.AsNumber());
                default:
                    
                    if(value.GetRawValue() is BinaryDataContext bdc)
                    {
                        System.Diagnostics.Debug.Assert(bdc != null);

                        return Convert.ToBase64String(bdc.Buffer, Base64FormattingOptions.InsertLineBreaks);
                    }
                    else
                    {
                        return value.GetRawValue().AsString();
                    }

            }
        }

        [ContextMethod("XMLЗначение", "XMLValue")]
        public IValue XMLValue(IValue givenType, string presentation)
        {
            if (givenType.GetRawValue().DataType != DataType.Type)
            {
                throw new ArgumentException(nameof(givenType));
            }

            var dataType = givenType.GetRawValue() as TypeTypeValue;
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
            else if (typeValue.Equals(BasicTypes.Undefined) && presentation == "")
            {
                return ValueFactory.Create();
            }
            else if (typeValue.ImplementingClass == typeof(BinaryDataContext))
            {
                byte[] bytes = Convert.FromBase64String(presentation);
                return new BinaryDataContext(bytes);
            }
            else
            {
                throw RuntimeException.InvalidArgumentValue();
            }

        }
        
        public static IAttachableContext CreateInstance()
        {
            return new XmlGlobalFunctions();
        }

    }
}
