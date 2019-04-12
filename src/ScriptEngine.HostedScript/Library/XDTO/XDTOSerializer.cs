/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.HostedScript.Library.Xml;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.HostedScript.Library.XMLSchema;

namespace ScriptEngine.HostedScript.Library.XDTO
{
    [ContextClass("СериализаторXDTO", "XDTOSerializer")]
    internal class XDTOSerializer : AutoContext<XDTOSerializer>
    {
        private static readonly XmlGlobalFunctions xmlGlobalFunctions = GlobalsManager.GetGlobalContext<XmlGlobalFunctions>();
        private XDTOSerializer() { }

        #region OneScript

        #region Properties

        [ContextProperty("Фабрика", "Factory")]
        public IValue Factory { get; }

        #endregion

        #region Methods

        [ContextMethod("XMLЗначение", "XMLValue")]
        public IValue XMLValue(IValue givenType, string presentation) => xmlGlobalFunctions.XMLValue(givenType, presentation);

        [ContextMethod("XMLСтрока", "XMLString")]
        public string XMLString(IValue value) => xmlGlobalFunctions.XMLString(value);

        //XMLТип(XMLType)
        //XMLТипЗнч(XMLTypeOf)
        //ВозможностьЧтенияXML(CanReadXML)
        //ЗаписатьJSON(WriteJSON)
        //ЗаписатьXDTO(WriteXDTO)

        [ContextMethod("ЗаписатьXML", "WriteXML")]
        public void WriteXML(XmlWriterImpl xmlWriter, IValue value, IValue typeAssigment, IValue Form)
        {
            switch (value.DataType)
            {
                case DataType.String:
                    xmlWriter.WriteStartElement("string", "http://www.w3.org/2001/XMLSchema");
                    xmlWriter.WriteText(value.AsString());
                    xmlWriter.WriteEndElement();
                    break;

                case DataType.Object:

                    IRuntimeContextInstance valueObject = value.AsObject();
                    if (valueObject is XMLSchema.XMLSchema seriazable)
                        seriazable.WriteXML(xmlWriter, valueObject);
                    else
                        throw RuntimeException.InvalidArgumentType();
                    break;

                default:
                    throw RuntimeException.InvalidArgumentType();
            }
        }

        //ИзXMLТипа(FromXMLType)
        //ПолучитьXMLТип(GetXMLType)
        //ПрочитатьJSON(ReadJSON)
        //ПрочитатьXDTO(ReadXDTO)

        [ContextMethod("ПрочитатьXML", "ReadXML")]
        public IValue ReadXML(XmlReaderImpl xmlReader, TypeTypeValue valueType)
        {
            System.Type typeValue = TypeManager.GetImplementingClass(valueType.Value.ID);

            if (typeValue.Equals(TypeDescriptor.FromDataType(DataType.String)))
            {
                return null;
            }
            else if (typeValue.Equals(TypeDescriptor.FromDataType(DataType.Object)))
            {
                return XMLSchemaImpl_Serialize.ReadXML(xmlReader);
            }
            else
                throw RuntimeException.InvalidArgumentType();
        }

        #endregion

        #region Constructors

        [ScriptConstructor(Name = "По умолчанию")]
        public static XDTOSerializer CreateInstance() => new XDTOSerializer();

        #endregion

        #endregion
    }
}
