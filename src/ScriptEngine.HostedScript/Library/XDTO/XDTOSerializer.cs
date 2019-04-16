/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Xml;
using System.Xml.Schema;
using ScriptEngine.HostedScript.Library.Xml;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XDTO
{
    [ContextClass("СериализаторXDTO", "XDTOSerializer")]
    internal class XDTOSerializer : AutoContext<XDTOSerializer>
    {
        private static readonly XmlGlobalFunctions xmlGlobalFunctions = GlobalsManager.GetGlobalContext<XmlGlobalFunctions>();
        private static readonly XmlNodeTypeEnum xmlNodeEnum = GlobalsManager.GetEnum<XmlNodeTypeEnum>();


        private XDTOSerializer() { }

        private void WriteXMLSimpleData(XmlWriterImpl xmlWriter, string name, string value, XMLExpandedName type, XMLTypeAssignment typeAssigment, XMLForm form)
        {
            XmlNamespaceContext namespaceContext;
            switch (form)
            {
                case XMLForm.Attribute:
                    namespaceContext = xmlWriter.NamespaceContext;
                    if (namespaceContext.LookupPrefix(XmlSchema.Namespace) == ValueFactory.Create())
                        xmlWriter.WriteNamespaceMapping("", XmlSchema.Namespace);

                    xmlWriter.WriteStartAttribute(name);
                    xmlWriter.WriteText(value);
                    xmlWriter.WriteEndAttribute();
                    break;

                case XMLForm.Text:
                    xmlWriter.WriteText(value);
                    break;

                default:
                    namespaceContext = xmlWriter.NamespaceContext;

                    xmlWriter.WriteStartElement(name);
                    if (namespaceContext.LookupPrefix(XmlSchema.Namespace) == ValueFactory.Create())
                        xmlWriter.WriteNamespaceMapping("", XmlSchema.Namespace);

                    if (namespaceContext.LookupPrefix(XmlSchema.InstanceNamespace) == ValueFactory.Create())
                        xmlWriter.WriteNamespaceMapping("xsi", XmlSchema.InstanceNamespace);

                    if (typeAssigment == XMLTypeAssignment.Explicit)
                    {
                        xmlWriter.WriteStartAttribute("type", type.NamespaceURI);
                        xmlWriter.WriteText(type.LocalName);
                        xmlWriter.WriteEndAttribute();
                    }

                    xmlWriter.WriteText(value);

                    xmlWriter.WriteEndElement();
                    break;
            }
        }

        private void WriteXMLUndefined(XmlWriterImpl xmlWriter, string name, XMLForm form)
        {
            if (form == XMLForm.Element)
            {
                XmlNamespaceContext namespaceContext = xmlWriter.NamespaceContext;
                if (namespaceContext.LookupPrefix(XmlSchema.Namespace) == ValueFactory.Create())
                    xmlWriter.WriteNamespaceMapping("", XmlSchema.Namespace);

                if (namespaceContext.LookupPrefix(XmlSchema.InstanceNamespace) == ValueFactory.Create())
                    xmlWriter.WriteNamespaceMapping("xsi", XmlSchema.InstanceNamespace);

                xmlWriter.WriteStartElement("Undefined");
                xmlWriter.WriteStartAttribute("nil", XmlSchema.InstanceNamespace);
                xmlWriter.WriteText("true");
                xmlWriter.WriteEndAttribute();
                xmlWriter.WriteEndElement();
            }
        }

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
        public void WriteXML(XmlWriterImpl xmlWriter, IValue value, XMLTypeAssignment typeAssigment = XMLTypeAssignment.Explicit, XMLForm form = XMLForm.Element)
        {
            XMLExpandedName xmlType;
            switch (value.DataType)
            {
                case DataType.Undefined:

                    WriteXMLUndefined(xmlWriter, "Undefined", form);
                    break;

                case DataType.String:

                    xmlType = new XMLExpandedName(XmlSchema.InstanceNamespace, "string");
                    WriteXMLSimpleData(xmlWriter, "string", value.AsString(), xmlType, typeAssigment, form);
                    break;

                case DataType.Number:

                    xmlType = new XMLExpandedName(XmlSchema.InstanceNamespace, "decimal");
                    WriteXMLSimpleData(xmlWriter, "decimal", XMLString(value), xmlType, typeAssigment, form);
                    break;

                case DataType.Object:

                    IRuntimeContextInstance valueObject = value.AsObject();
                    if (valueObject is IXDTOSerializableXML seriazable)
                        seriazable.WriteXML(xmlWriter);
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
        public IValue ReadXML(XmlReaderImpl xmlReader, IValue valueType)
        {
            TypeDescriptor typeValue = TypeManager.GetTypeDescriptorFor(valueType.GetRawValue());

            if (typeValue.Equals(TypeDescriptor.FromDataType(DataType.String)))
            {
                xmlReader.Read();
                if (xmlReader.NodeType == xmlNodeEnum.FromNativeValue(XmlNodeType.Text))
                    return ValueFactory.Create(xmlReader.Value);
                else
                    throw RuntimeException.InvalidArgumentValue();
            }
            //else if (typeValue.Equals(TypeDescriptor.FromDataType(DataType.Object)))
            //{
            //    return new XMLSchema.XMLSchema(xmlReader);
            //}
            //else
            //    throw RuntimeException.InvalidArgumentType();
            return ValueFactory.Create();
        }

        #endregion

        #region Constructors

        [ScriptConstructor(Name = "По умолчанию")]
        public static XDTOSerializer CreateInstance() => new XDTOSerializer();

        #endregion

        #endregion
    }
}
