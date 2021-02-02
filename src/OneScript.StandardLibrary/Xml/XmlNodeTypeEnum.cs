/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;

namespace OneScript.StandardLibrary.Xml
{
    [SystemEnum("ТипУзлаXML", "XMLNodeType")]
    public class XmlNodeTypeEnum : EnumerationContext
    {
        readonly Dictionary<XmlNodeType, IValue> _valuesCache = new Dictionary<XmlNodeType,IValue>();

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
                val = this.ValuesInternal.First(x => ((ClrEnumValueWrapper<XmlNodeType>)x).UnderlyingValue == native);
                _valuesCache.Add(native, val);
            }

            return val;
        }

        public static XmlNodeTypeEnum CreateInstance(ITypeManager typeManager)
        {
            var instance = EnumContextHelper.CreateClrEnumInstance<XmlNodeTypeEnum, XmlNodeType>(
                typeManager,
                (t,v) => new XmlNodeTypeEnum(t, v));

            instance.WrapClrValue("Атрибут", "Attribute", XmlNodeType.Attribute);
            instance.WrapClrValue("ИнструкцияОбработки", "ProcessingInstruction", XmlNodeType.ProcessingInstruction);
            instance.WrapClrValue("Комментарий", "Comment", XmlNodeType.Comment);
            instance.WrapClrValue("КонецСущности", "EndEntity", XmlNodeType.EndEntity);
            instance.WrapClrValue("КонецЭлемента", "EndElement", XmlNodeType.EndElement);
            instance.WrapClrValue("НачалоЭлемента", "StartElement", XmlNodeType.Element);
            instance.WrapClrValue("Ничего", "None", XmlNodeType.None);
            instance.WrapClrValue("Нотация", "Notation", XmlNodeType.Notation);
            instance.WrapClrValue("ОбъявлениеXML", "XMLDeclaration", XmlNodeType.XmlDeclaration);
            instance.WrapClrValue("ОпределениеТипаДокумента", "DocumentTypeDefinition", XmlNodeType.DocumentType);
            instance.WrapClrValue("ПробельныеСимволы", "Whitespace", XmlNodeType.Whitespace);
            instance.WrapClrValue("СекцияCDATA", "CDATASection", XmlNodeType.CDATA);
            instance.WrapClrValue("СсылкаНаСущность", "EntityReference", XmlNodeType.EntityReference);
            instance.WrapClrValue("Сущность", "Entity", XmlNodeType.Entity);
            instance.WrapClrValue("Текст", "Text", XmlNodeType.Text);

            return instance;
        }
   }
}
