using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace ScriptEngine.HostedScript.Library.Xml
{
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

        private static XmlNodeTypeEnum _instance;

        public static XmlNodeTypeEnum GetInstance()
        {
            if (_instance == null)
            {
                var type = TypeManager.RegisterType("ПеречислениеТипУзлаXML", typeof(XmlNodeTypeEnum));
                var enumValueType = TypeManager.RegisterType("ТипУзлаXML", typeof(CLREnumValueWrapper<XmlNodeType>));

                _instance = new XmlNodeTypeEnum(type, enumValueType);

                _instance.AddValue("Атрибут", new CLREnumValueWrapper<XmlNodeType>(_instance, XmlNodeType.Attribute));
                _instance.AddValue("ИнструкцияОбработки", new CLREnumValueWrapper<XmlNodeType>(_instance, XmlNodeType.ProcessingInstruction));
                _instance.AddValue("Комментарий", new CLREnumValueWrapper<XmlNodeType>(_instance, XmlNodeType.Comment));
                _instance.AddValue("КонецСущности", new CLREnumValueWrapper<XmlNodeType>(_instance, XmlNodeType.EndEntity));
                _instance.AddValue("КонецЭлемента", new CLREnumValueWrapper<XmlNodeType>(_instance, XmlNodeType.EndElement));
                _instance.AddValue("НачалоЭлемента", new CLREnumValueWrapper<XmlNodeType>(_instance, XmlNodeType.Element));
                _instance.AddValue("Ничего", new CLREnumValueWrapper<XmlNodeType>(_instance, XmlNodeType.None));
                _instance.AddValue("Нотация", new CLREnumValueWrapper<XmlNodeType>(_instance, XmlNodeType.Notation));
                _instance.AddValue("Объявление", new CLREnumValueWrapper<XmlNodeType>(_instance, XmlNodeType.XmlDeclaration));
                _instance.AddValue("ОпределениеТипаДокумента", new CLREnumValueWrapper<XmlNodeType>(_instance, XmlNodeType.DocumentType));
                _instance.AddValue("ПробельныеСимволы", new CLREnumValueWrapper<XmlNodeType>(_instance, XmlNodeType.Whitespace));
                _instance.AddValue("СекцияCDATA", new CLREnumValueWrapper<XmlNodeType>(_instance, XmlNodeType.CDATA));
                _instance.AddValue("СсылкаНаСущность", new CLREnumValueWrapper<XmlNodeType>(_instance, XmlNodeType.EntityReference));
                _instance.AddValue("Сущность", new CLREnumValueWrapper<XmlNodeType>(_instance, XmlNodeType.Entity));
                _instance.AddValue("Текст", new CLREnumValueWrapper<XmlNodeType>(_instance, XmlNodeType.Text));

                _instance["Атрибут"].ValuePresentation = "Атрибут";                
                _instance["ИнструкцияОбработки"].ValuePresentation = "ИнструкцияОбработки";
                _instance["Комментарий"].ValuePresentation = "Комментарий";
                _instance["КонецСущности"].ValuePresentation = "КонецСущности";
                _instance["КонецЭлемента"].ValuePresentation = "КонецЭлемента";
                _instance["НачалоЭлемента"].ValuePresentation = "НачалоЭлемента";
                _instance["Ничего"].ValuePresentation = "Ничего";
                _instance["Нотация"].ValuePresentation = "Нотация";
                _instance["Объявление"].ValuePresentation = "Объявление";
                _instance["ОпределениеТипаДокумента"].ValuePresentation = "ОпределениеТипаДокумента";
                _instance["ПробельныеСимволы"].ValuePresentation = "ПробельныеСимволы";
                _instance["СекцияCDATA"].ValuePresentation = "СекцияCDATA";
                _instance["СсылкаНаСущность"].ValuePresentation = "СсылкаНаСущность";
                _instance["Сущность"].ValuePresentation = "Сущность";
                _instance["Текст"].ValuePresentation = "Текст";


            }

            return _instance;
        }

   }
}
