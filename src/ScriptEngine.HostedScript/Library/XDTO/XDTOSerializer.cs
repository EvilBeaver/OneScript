/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
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

        private void WriteXMLSimpleData(XmlWriterImpl xmlWriter,
                                        string name,
                                        IValue value,
                                        XMLExpandedName type,
                                        XMLTypeAssignment typeAssigment,
                                        XMLForm form)
        {
            XmlNamespaceContext namespaceContext;
            string xmlValue = XMLString(value);
            switch (form)
            {
                case XMLForm.Attribute:
                    namespaceContext = xmlWriter.NamespaceContext;
                    AddNamespaceMapping(namespaceContext, xmlWriter, "", XmlSchema.Namespace);

                    xmlWriter.WriteStartAttribute(name);
                    xmlWriter.WriteText(xmlValue);
                    xmlWriter.WriteEndAttribute();
                    break;

                case XMLForm.Text:
                    xmlWriter.WriteText(xmlValue);
                    break;

                default:

                    xmlWriter.WriteStartElement(name);

                    namespaceContext = xmlWriter.NamespaceContext;
                    AddNamespaceMapping(namespaceContext, xmlWriter, "", XmlSchema.Namespace);
                    AddNamespaceMapping(namespaceContext, xmlWriter, "xsi", XmlSchema.InstanceNamespace);

                    if (typeAssigment == XMLTypeAssignment.Explicit)
                    {
                        xmlWriter.WriteStartAttribute("type", XmlSchema.InstanceNamespace);
                        xmlWriter.WriteText(type.LocalName);
                        xmlWriter.WriteEndAttribute();
                    }

                    xmlWriter.WriteText(xmlValue);

                    xmlWriter.WriteEndElement();
                    break;
            }
        }

        private void WriteXMLUndefined(XmlWriterImpl xmlWriter, string name, XMLForm form)
        {
            if (form == XMLForm.Element)
            {
                XmlNamespaceContext namespaceContext = xmlWriter.NamespaceContext;
                AddNamespaceMapping(namespaceContext, xmlWriter, "", XmlSchema.Namespace);
                AddNamespaceMapping(namespaceContext, xmlWriter, "xsi", XmlSchema.InstanceNamespace);

                xmlWriter.WriteStartElement(name);
                xmlWriter.WriteStartAttribute("nil", XmlSchema.InstanceNamespace);
                xmlWriter.WriteText("true");
                xmlWriter.WriteEndAttribute();
                xmlWriter.WriteEndElement();
            }
        }

        private void AddNamespaceMapping(XmlNamespaceContext namespaceContext, XmlWriterImpl xmlWriter, string prefix, string uri)
        {
            if (namespaceContext.LookupPrefix(uri) == ValueFactory.Create())
                xmlWriter.WriteNamespaceMapping(prefix, uri);
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
        public void WriteXML(XmlWriterImpl xmlWriter,
                             IValue value,
                             XMLTypeAssignment typeAssigment = XMLTypeAssignment.Implicit,
                             XMLForm form = XMLForm.Element)
        {
            switch (value.DataType)
            {
                case DataType.Undefined:

                    WriteXML(xmlWriter, value, "Undefined", typeAssigment, form);
                    break;

                case DataType.String:

                    WriteXML(xmlWriter, value, "string", typeAssigment, form);
                    break;

                case DataType.Number:

                    WriteXML(xmlWriter, value, "decimal", typeAssigment, form);
                    break;

                case DataType.Boolean:

                    WriteXML(xmlWriter, value, "boolean", typeAssigment, form);
                    break;

                case DataType.Date:

                    WriteXML(xmlWriter, value, "dateTime", typeAssigment, form);
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

        public void WriteXML(XmlWriterImpl xmlWriter,
                             IValue value,
                             string name,
                             XMLTypeAssignment typeAssigment = XMLTypeAssignment.Implicit,
                             XMLForm form = XMLForm.Element)
        {
            XMLExpandedName xmlType;
            switch (value.DataType)
            {
                case DataType.Undefined:

                    WriteXMLUndefined(xmlWriter, name, form);
                    break;

                case DataType.String:

                    xmlType = new XMLExpandedName(XmlSchema.InstanceNamespace, "string");
                    WriteXMLSimpleData(xmlWriter, name, value, xmlType, typeAssigment, form);
                    break;

                case DataType.Number:

                    xmlType = new XMLExpandedName(XmlSchema.InstanceNamespace, "decimal");
                    WriteXMLSimpleData(xmlWriter, name, value, xmlType, typeAssigment, form);
                    break;

                case DataType.Boolean:

                    xmlType = new XMLExpandedName(XmlSchema.InstanceNamespace, "boolean");
                    WriteXMLSimpleData(xmlWriter, name, value, xmlType, typeAssigment, form);
                    break;

                case DataType.Date:

                    xmlType = new XMLExpandedName(XmlSchema.InstanceNamespace, "dateTime");
                    WriteXMLSimpleData(xmlWriter, name, value, xmlType, typeAssigment, form);
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
        public IValue ReadXML(XmlReaderImpl xmlReader, IValue valueType = null)
        {
            TypeTypeValue typeValue = null;

            if (valueType is TypeTypeValue typeTypeValue)
                typeValue = typeTypeValue;

            else if (xmlReader.NodeType == xmlNodeEnum.FromNativeValue(XmlNodeType.Element))
            {
                IValue xsiType = xmlReader.GetAttribute(ValueFactory.Create("type"), XmlSchema.InstanceNamespace);
                IValue xsiNil = xmlReader.GetAttribute(ValueFactory.Create("nil"), XmlSchema.InstanceNamespace);

                if (xsiType.DataType == DataType.String)
                {
                    switch (xsiType.AsString())
                    {
                        case "string":
                            typeValue = new TypeTypeValue("String");
                            break;

                        case "decimal":
                            typeValue = new TypeTypeValue("Number");
                            break;

                        case "boolean":
                            typeValue = new TypeTypeValue("Boolean");
                            break;

                        case "dateTime":
                            typeValue = new TypeTypeValue("Date");
                            break;

                        default:
                            break;
                    }
                }
                else if(xsiNil.DataType == DataType.String)
                    typeValue = new TypeTypeValue("Undefined");
            };

            if (typeValue == null)
                throw RuntimeException.InvalidArgumentValue();

            Type implType = TypeManager.GetImplementingClass(typeValue.Value.ID);

            IValue result = ValueFactory.Create();

            if (typeValue.Equals(new TypeTypeValue("Undefined")))
            {
                result = ValueFactory.Create();
                xmlReader.Skip();
            }
            else if (implType == typeof(DataType))
            {
                xmlReader.Read();
                if (xmlReader.NodeType == xmlNodeEnum.FromNativeValue(XmlNodeType.Text))
                {
                    result = XMLValue(typeValue, xmlReader.Value);
                    xmlReader.Read();
                }
                else
                    throw RuntimeException.InvalidArgumentValue();
            }
            else if (typeof(IXDTOSerializableXML).IsAssignableFrom(implType))
                result = Activator.CreateInstance(implType, xmlReader) as IValue;

            xmlReader.Read();
            return result;
        }

        #endregion

        #region Constructors

        [ScriptConstructor(Name = "По умолчанию")]
        public static XDTOSerializer CreateInstance() => new XDTOSerializer();

        #endregion

        #endregion
    }
}
