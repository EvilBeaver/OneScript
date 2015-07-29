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

namespace ScriptEngine.HostedScript.Library.Xml
{
    [SystemEnum("ТипУзлаXML", "XMLNodeType")]
    public class XmlNodeTypeEnum : EnumerationContext
    {
        Dictionary<XmlNodeType, IValue> _valuesCache = new Dictionary<XmlNodeType,IValue>();

        private XmlNodeTypeEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {

        }

        public IValue FromNativeValue(XmlNodeType native)
        {
            if (native == XmlNodeType.SignificantWhitespace)
                native = XmlNodeType.Whitespace;

            IValue val;
            if(_valuesCache.TryGetValue(native, out val))
            {
                return val;
            }
            else
            {
                val = this.ValuesInternal.First(x => ((CLREnumValueWrapper<XmlNodeType>)x).UnderlyingObject == native);
                _valuesCache.Add(native, val);
            }

            return val;
        }

        public static XmlNodeTypeEnum CreateInstance()
        {
            XmlNodeTypeEnum instance;
            var type = TypeManager.RegisterType("ПеречислениеТипУзлаXML", typeof(XmlNodeTypeEnum));
            var enumValueType = TypeManager.RegisterType("ТипУзлаXML", typeof(CLREnumValueWrapper<XmlNodeType>));

            instance = new XmlNodeTypeEnum(type, enumValueType);

            instance.AddValue("Атрибут", "Attribute", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.Attribute));
            instance.AddValue("ИнструкцияОбработки", "ProcessingInstruction", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.ProcessingInstruction));
            instance.AddValue("Комментарий", "Comment", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.Comment));
            instance.AddValue("КонецСущности", "EndEntity", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.EndEntity));
            instance.AddValue("КонецЭлемента", "EndElement", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.EndElement));
            instance.AddValue("НачалоЭлемента", "StartElement", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.Element));
            instance.AddValue("Ничего", "None", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.None));
            instance.AddValue("Нотация", "Notation", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.Notation));
            instance.AddValue("ОбъявлениеXML", "XMLDeclaration", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.XmlDeclaration));
            instance.AddValue("ОпределениеТипаДокумента", "DocumentTypeDefinition", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.DocumentType));
            instance.AddValue("ПробельныеСимволы", "Whitespace", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.Whitespace));
            instance.AddValue("СекцияCDATA", "CDATASection", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.CDATA));
            instance.AddValue("СсылкаНаСущность", "EntityReference", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.EntityReference));
            instance.AddValue("Сущность", "Entity", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.Entity));
            instance.AddValue("Текст", "Text", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.Text));

            return instance;
        }
   }
}
