/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Diagnostics;
using System.Xml;
using OneScript.Commons;
using OneScript.StandardLibrary.Binary;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Xml
{
    [GlobalContext(Category="Функции работы с XML")]
    public class XmlGlobalFunctions : GlobalContextBase<XmlGlobalFunctions>
    {
        [ContextMethod("XMLСтрока", "XMLString")]
        public string XMLString(IValue value)
        {
            if (value.SystemType == BasicTypes.Undefined)
                return "";
            else if(value.SystemType == BasicTypes.Boolean)
                return XmlConvert.ToString(value.AsBoolean());
            else if(value.SystemType == BasicTypes.Date)
                return XmlConvert.ToString(value.AsDate(), XmlDateTimeSerializationMode.Unspecified);
            else if(value.SystemType == BasicTypes.Number)
                return XmlConvert.ToString(value.AsNumber());
            else
            {
                if(value.GetRawValue() is BinaryDataContext bdc)
                {
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
            if (givenType.GetRawValue().SystemType != BasicTypes.Type)
            {
                throw new ArgumentException(nameof(givenType));
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
