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

            instance.AddValue("Атрибут", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.Attribute));
            instance.AddValue("ИнструкцияОбработки", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.ProcessingInstruction));
            instance.AddValue("Комментарий", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.Comment));
            instance.AddValue("КонецСущности", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.EndEntity));
            instance.AddValue("КонецЭлемента", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.EndElement));
            instance.AddValue("НачалоЭлемента", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.Element));
            instance.AddValue("Ничего", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.None));
            instance.AddValue("Нотация", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.Notation));
            instance.AddValue("Объявление", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.XmlDeclaration));
            instance.AddValue("ОпределениеТипаДокумента", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.DocumentType));
            instance.AddValue("ПробельныеСимволы", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.Whitespace));
            instance.AddValue("СекцияCDATA", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.CDATA));
            instance.AddValue("СсылкаНаСущность", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.EntityReference));
            instance.AddValue("Сущность", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.Entity));
            instance.AddValue("Текст", new CLREnumValueWrapper<XmlNodeType>(instance, XmlNodeType.Text));

            return instance;
        }
   }
}
