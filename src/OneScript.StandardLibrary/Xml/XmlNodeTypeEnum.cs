/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;

namespace OneScript.StandardLibrary.Xml
{
    [SystemEnum("ТипУзлаXML", "XMLNodeType")]
    public class XmlNodeTypeEnum : ClrEnumWrapper<XmlNodeType>
    {
        readonly Dictionary<XmlNodeType, ClrEnumValueWrapper<XmlNodeType>> _valuesCache = new Dictionary<XmlNodeType,ClrEnumValueWrapper<XmlNodeType>>();

        private XmlNodeTypeEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
            MakeValue("Атрибут", "Attribute", XmlNodeType.Attribute);
            MakeValue("ИнструкцияОбработки", "ProcessingInstruction", XmlNodeType.ProcessingInstruction);
            MakeValue("Комментарий", "Comment", XmlNodeType.Comment);
            MakeValue("КонецСущности", "EndEntity", XmlNodeType.EndEntity);
            MakeValue("КонецЭлемента", "EndElement", XmlNodeType.EndElement);
            MakeValue("НачалоЭлемента", "StartElement", XmlNodeType.Element);
            MakeValue("Ничего", "None", XmlNodeType.None);
            MakeValue("Нотация", "Notation", XmlNodeType.Notation);
            MakeValue("ОбъявлениеXML", "XMLDeclaration", XmlNodeType.XmlDeclaration);
            MakeValue("ОпределениеТипаДокумента", "DocumentTypeDefinition", XmlNodeType.DocumentType);
            MakeValue("ПробельныеСимволы", "Whitespace", XmlNodeType.Whitespace);
            MakeValue("СекцияCDATA", "CDATASection", XmlNodeType.CDATA);
            MakeValue("СсылкаНаСущность", "EntityReference", XmlNodeType.EntityReference);
            MakeValue("Сущность", "Entity", XmlNodeType.Entity);
            MakeValue("Текст", "Text", XmlNodeType.Text);
        }

        private void MakeValue(string name, string alias, XmlNodeType enumValue)
        {
            var wrappedValue = this.WrapClrValue(name, alias, enumValue);
            _valuesCache[enumValue] = wrappedValue;
        }

        public override ClrEnumValueWrapper<XmlNodeType> FromNativeValue(XmlNodeType native)
        {
            if (native == XmlNodeType.SignificantWhitespace)
                native = XmlNodeType.Whitespace;

            return _valuesCache[native];
        }

        public static XmlNodeTypeEnum CreateInstance(ITypeManager typeManager)
        {
            var instance = EnumContextHelper.CreateClrEnumInstance<XmlNodeTypeEnum, XmlNodeType>(
                typeManager,
                (t,v) => new XmlNodeTypeEnum(t, v));

            return instance;
        }
   }
}
