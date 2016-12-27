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
using System.Linq;
using System.Text;
using System.Xml;

using ScriptEngine.HostedScript.Library.Binary;

namespace ScriptEngine.HostedScript.Library.Xml
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
                    
                    if(value.SystemType.Equals(TypeManager.GetTypeByFrameworkType(typeof(BinaryDataContext))))
                    {
                        var bdc = value.GetRawValue() as BinaryDataContext;
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
            else if (typeValue.Equals(TypeDescriptor.FromDataType(DataType.Undefined)) && presentation == "")
            {
                return ValueFactory.Create();
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
