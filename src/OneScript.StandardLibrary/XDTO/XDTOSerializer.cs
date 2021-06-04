/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Xml;
using System.Xml.Schema;
using OneScript.Commons;
using OneScript.StandardLibrary.Xml;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.XDTO
{
    [ContextClass("СериализаторXDTO", "XDTOSerializer")]
    public class XDTOSerializer : AutoContext<XDTOSerializer>
    {
        private readonly ITypeManager _typeManager;
        private readonly XmlGlobalFunctions _xmlGlobalFunctions;
        private readonly XmlNodeTypeEnum _xmlNodeEnum;


        private XDTOSerializer(ITypeManager typeManager, IGlobalsManager globalsManager)
        {
            _typeManager = typeManager;
            _xmlGlobalFunctions = globalsManager.GetInstance<XmlGlobalFunctions>();
            _xmlNodeEnum = globalsManager.GetInstance<XmlNodeTypeEnum>();
        }

        private void WriteXMLSimpleData(XmlWriterImpl xmlWriter,
                                        string name,
                                        string value,
                                        XMLExpandedName type,
                                        XMLTypeAssignment typeAssigment,
                                        XMLForm form)
        {
            XmlNamespaceContext namespaceContext;
            switch (form)
            {
                case XMLForm.Attribute:
                    namespaceContext = xmlWriter.NamespaceContext;
                    AddNamespaceMapping(namespaceContext, xmlWriter, "", XmlSchema.Namespace);

                    xmlWriter.WriteStartAttribute(name);
                    xmlWriter.WriteText(value);
                    xmlWriter.WriteEndAttribute();
                    break;

                case XMLForm.Text:
                    xmlWriter.WriteText(value);
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
        public IValue XMLValue(IValue givenType, string presentation) => _xmlGlobalFunctions.XMLValue(givenType, presentation);

        [ContextMethod("XMLСтрока", "XMLString")]
        public string XMLString(IValue value) => _xmlGlobalFunctions.XMLString(value);

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
            value = value.GetRawValue();
            if (value.SystemType == BasicTypes.Undefined)
            {
                WriteXML(xmlWriter, null, "Undefined", typeAssigment, form);
            }
            else if (value.SystemType == BasicTypes.String)
            {
                WriteXML(xmlWriter, XMLString(value), "string", typeAssigment, form);
            }
            else if (value.SystemType == BasicTypes.Number)
            {
                WriteXML(xmlWriter, XMLString(value), "decimal", typeAssigment, form);
            }
            else if (value.SystemType == BasicTypes.Boolean)
            {
                WriteXML(xmlWriter, XMLString(value), "boolean", typeAssigment, form);
            }
            else if (value.SystemType == BasicTypes.Date)
            {
                WriteXML(xmlWriter, XMLString(value), "dateTime", typeAssigment, form);
            }
            else
            {
                if(!(value is IXDTOSerializableXML seriazable))
                    throw RuntimeException.InvalidArgumentType();
                
                seriazable.WriteXML(xmlWriter, this);
            }
        }

        private void WriteXML(XmlWriterImpl xmlWriter,
            string xmlString,
            string name,
            XMLTypeAssignment typeAssigment = XMLTypeAssignment.Implicit,
            XMLForm form = XMLForm.Element)
        {
            XMLExpandedName xmlType;
            if (name == "Undefined")
            {
                WriteXMLUndefined(xmlWriter, name, form);
            }
            else
            {
                xmlType = new XMLExpandedName(XmlSchema.InstanceNamespace, name);
                WriteXMLSimpleData(xmlWriter, name, xmlString, xmlType, typeAssigment, form);
            }

        }

        //ИзXMLТипа(FromXMLType)
        //ПолучитьXMLТип(GetXMLType)
        //ПрочитатьJSON(ReadJSON)
        //ПрочитатьXDTO(ReadXDTO)

        [ContextMethod("ПрочитатьXML", "ReadXML")]
        public IValue ReadXML(XmlReaderImpl xmlReader, IValue valueType = null)
        {
            BslTypeValue typeValue = null;

            if (valueType is BslTypeValue typeTypeValue)
                typeValue = typeTypeValue;

            else if (xmlReader.NodeType == _xmlNodeEnum.FromNativeValue(XmlNodeType.Element))
            {
                IValue xsiType = xmlReader.GetAttribute(ValueFactory.Create("type"), XmlSchema.InstanceNamespace);
                IValue xsiNil = xmlReader.GetAttribute(ValueFactory.Create("nil"), XmlSchema.InstanceNamespace);

                if (xsiType.SystemType == BasicTypes.String)
                {
                    switch (xsiType.AsString())
                    {
                        case "string":
                            typeValue = new BslTypeValue(BasicTypes.String);
                            break;

                        case "decimal":
                            typeValue = new BslTypeValue(BasicTypes.Number);
                            break;

                        case "boolean":
                            typeValue = new BslTypeValue(BasicTypes.Boolean);
                            break;

                        case "dateTime":
                            typeValue = new BslTypeValue(BasicTypes.Number);
                            break;

                        default:
                            break;
                    }
                }
                else if (xsiNil.SystemType == BasicTypes.String)
                    typeValue = new BslTypeValue(BasicTypes.Undefined);
            };

            if (typeValue == null)
                throw RuntimeException.InvalidArgumentValue();

            Type implType = typeValue.TypeValue.ImplementingClass;

            IValue result = ValueFactory.Create();

            if (typeValue.Equals(new BslTypeValue(BasicTypes.Undefined)))
            {
                result = ValueFactory.Create();
                xmlReader.Skip();
            }
            else if (implType == typeof(DataType)) // TODO: такого не должно быть.
            {
                xmlReader.Read();
                if (xmlReader.NodeType == _xmlNodeEnum.FromNativeValue(XmlNodeType.Text))
                {
                    result = XMLValue(typeValue, xmlReader.Value);
                    xmlReader.Read();
                }
                else
                    throw RuntimeException.InvalidArgumentValue();
            }
            else if (typeof(IXDTOSerializableXML).IsAssignableFrom(implType))
                result = Activator.CreateInstance(implType, new object[] { xmlReader, this }) as IValue;

            xmlReader.Read();
            return result;
        }

        #endregion

        #region Constructors

        [ScriptConstructor(Name = "По умолчанию")]
        public static XDTOSerializer CreateInstance(TypeActivationContext context)
        {
            var globalsManager = context.Services.Resolve<IGlobalsManager>();
            return new XDTOSerializer(context.TypeManager, globalsManager);
        }

        #endregion

        #endregion
    }
}
