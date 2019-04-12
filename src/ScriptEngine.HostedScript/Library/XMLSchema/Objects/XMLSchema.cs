/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using ScriptEngine.HostedScript.Library.Xml;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("СхемаXML", "XMLSchema")]
    public class XMLSchema : AutoContext<XMLSchema>, IXSComponent
    {
        private readonly XmlSchema _schema;

        private XMLSchema()
        {
            _schema = new XmlSchema();
            Components = new XSComponentFixedList();
            Annotations = new XSComponentFixedList();

            Directives = new XSComponentList();
            Directives.Inserted += DirectivesInserted;
            Directives.Cleared += DirectivesCleared;

            Content = new XSComponentList();
            Content.Inserted += ContentInserted;
            Content.Cleared += ContentCleared;

            BlockDefault = new XSDisallowedSubstitutionsUnion();
            FinalDefault = new XSSchemaFinalUnion();

            AttributeDeclarations = new XSNamedComponentMap();
            AttributeGroupDefinitions = new XSNamedComponentMap();
            NotationDeclarations = new XSNamedComponentMap();
            ElementDeclarations = new XSNamedComponentMap();
            ModelGroupDefinitions = new XSNamedComponentMap();
            IdentityConstraintDefinitions = new XSNamedComponentMap();
            TypeDefinitions = new XSNamedComponentMap();
        }

        internal XMLSchema(XmlSchema schema) : this()
        {
            _schema = schema;
        }

        private static string XMLText(XmlSchema xmlSchema)
        {
            if (!xmlSchema.IsCompiled)
            {
                XmlSchemaSet schemaSet = new XmlSchemaSet();
                schemaSet.ValidationEventHandler += new ValidationEventHandler(ValidationCallbackOne);
                schemaSet.Add(xmlSchema);
                schemaSet.Compile();
            }

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8
            };

            StringWriter strWriter = new StringWriter();

            XmlWriter XmlWriter = XmlWriter.Create(strWriter, settings);

            xmlSchema.Write(XmlWriter);

            XmlWriter.Close();

            return strWriter.ToString();
        }

        private static void ValidationCallbackOne(object sender, ValidationEventArgs args)
        {
            switch (args.Severity)
            {
                case XmlSeverityType.Error:
                    throw new RuntimeException(args.Message);

                case XmlSeverityType.Warning:
                    SystemLogger.Write($"WARNING:{args.Message}");
                    break;

                default:
                    break;
            }
        }

        #region OneScript

        #region Properties

        [ContextProperty("Аннотация", "Annotation")]
        public XSAnnotation Annotation => null;

        [ContextProperty("Компоненты", "Components")]
        public XSComponentFixedList Components { get; }

        [ContextProperty("Контейнер", "Container")]
        public IXSComponent Container => this;

        [ContextProperty("КорневойКонтейнер", "RootContainer")]
        public IXSComponent RootContainer => this;

        [ContextProperty("Схема", "Schema")]
        public XMLSchema Schema => this;

        [ContextProperty("ТипКомпоненты", "ComponentType")]
        public XSComponentType ComponentType => XSComponentType.Schema;

        [ContextProperty("ЭлементDOM", "DOMElement")]
        public IValue DOMElement => null;

        [ContextProperty("URIПространстваИменСхемыДляСхемыXML", "SchemaForSchemaNamespaceURI")]
        public string Namespace => XmlSchema.Namespace;

        [ContextProperty("Аннотации", "Annotations")]
        public XSComponentFixedList Annotations { get; }

        [ContextProperty("БлокировкаПоУмолчанию", "BlockDefault")]
        public XSDisallowedSubstitutionsUnion BlockDefault { get; }

        [ContextProperty("Версия", "Version")]
        public string Version
        {
            get => _schema.Version;
            set => _schema.Version = value;
        }

        [ContextProperty("Директивы", "Directives")]
        public XSComponentList Directives { get; }

        [ContextProperty("ДокументDOM", "DOMDocument")]
        public IValue DOMDocument => null;

        [ContextProperty("ЗавершенностьПоУмолчанию", "FinalDefault")]
        public XSSchemaFinalUnion FinalDefault { get; }

        [ContextProperty("ОбъявленияАтрибутов", "AttributeDeclarations")]
        public XSNamedComponentMap AttributeDeclarations { get; }

        [ContextProperty("ОбъявленияНотаций", "NotationDeclarations")]
        public XSNamedComponentMap NotationDeclarations { get; }

        [ContextProperty("ОбъявленияЭлементов", "ElementDeclarations")]
        public XSNamedComponentMap ElementDeclarations { get; }

        [ContextProperty("ОпределенияГруппАтрибутов", "AttributeGroupDefinitions")]
        public XSNamedComponentMap AttributeGroupDefinitions { get; }

        [ContextProperty("ОпределенияГруппМоделей", "ModelGroupDefinitions")]
        public XSNamedComponentMap ModelGroupDefinitions { get; }

        [ContextProperty("ОпределенияОграниченийИдентичности", "IdentityConstraintDefinitions")]
        public XSNamedComponentMap IdentityConstraintDefinitions { get; }

        [ContextProperty("ОпределенияТипов", "TypeDefinitions")]
        public XSNamedComponentMap TypeDefinitions { get; }

        [ContextProperty("ПрефиксСхемыДляСхемыXML", "SchemaForSchemaPrefix")]
        public string SchemaForSchemaPrefix { get; }

        [ContextProperty("ПространствоИмен", "TargetNamespace")]
        public string TargetNamespace
        {
            get => _schema.TargetNamespace;
            set => _schema.TargetNamespace = value;
        }

        [ContextProperty("РасположениеСхемы", "SchemaLocation")]
        public string SchemaLocation { get; set; }

        [ContextProperty("Содержимое", "Content")]
        public XSComponentList Content { get; }

        [ContextProperty("СхемаДляСхемыXML", "SchemaForSchema")]
        public XMLSchema SchemaForSchema { get; }

        [ContextProperty("ФормаАтрибутовПоУмолчанию", "AttributeFormDefault")]
        public XSForm AttributeFormDefault
        {
            get => XSForm.FromNativeValue(_schema.AttributeFormDefault);
            set => _schema.AttributeFormDefault = XSForm.ToNativeValue(value);
        }

        [ContextProperty("ФормаЭлементовПоУмолчанию", "ElementFormDefault")]
        public XSForm ElementFormDefault
        {
            get => XSForm.FromNativeValue(_schema.ElementFormDefault);
            set => _schema.ElementFormDefault = XSForm.ToNativeValue(value);
        }

        [ContextProperty("Язык", "Lang")]
        public string Lang { get; set; }

        #endregion

        #region Methods

        [ContextMethod("КлонироватьКомпоненту", "CloneComponent")]
        public IXSComponent CloneComponent(bool recursive = true) => throw new NotImplementedException();

        [ContextMethod("ОбновитьЭлементDOM", "UpdateDOMElement")]
        public void UpdateDOMElement() => throw new NotImplementedException();

        [ContextMethod("Содержит", "Contains")]
        public bool Contains(IXSComponent component) => Components.Contains(component);

        [ContextMethod("ТекстXML", "XMLText")]
        public string XMLText() => XMLText(_schema);

        #endregion

        #region Constructors

        [ScriptConstructor(Name = "По умолчанию")]
        public static XMLSchema Constructor() => new XMLSchema();

        #endregion

        #endregion

        #region IXSComponent

        XmlSchemaObject IXSComponent.SchemaObject => _schema;
        public XmlSchema SchemaObject => _schema;

        void IXSComponent.BindToContainer(IXSComponent RootContainer, IXSComponent Container)
            => throw new NotImplementedException();

        #endregion

        #region  XSComponentListEvents

        private void ContentInserted(object sender, XSComponentListEventArgs e)
        {
            IXSComponent component = e.Component;

            component.BindToContainer(this, this);
            Components.Add(component);
            _schema.Items.Add(component.SchemaObject);

            if (component is IXSNamedComponent)
                AddNamedComponentToList(component as IXSNamedComponent);
        }

        private void DirectivesInserted(object sender, XSComponentListEventArgs e)
        {
            IXSComponent component = e.Component;

            component.BindToContainer(this, this);
            Components.Add(component);

            _schema.Includes.Add(component.SchemaObject);
        }

        private void ContentCleared(object sender, EventArgs e)
        {
            Components.RemoveAll(x => (!(x is IXSDirective)));

            AttributeDeclarations.Clear();
            AttributeGroupDefinitions.Clear();
            NotationDeclarations.Clear();
            ElementDeclarations.Clear();
            ModelGroupDefinitions.Clear();
            IdentityConstraintDefinitions.Clear();
            TypeDefinitions.Clear();

            _schema.Items.Clear();
        }

        private void DirectivesCleared(object sender, EventArgs e)
        {
            Components.RemoveAll(x => (x is IXSDirective));
            _schema.Includes.Clear();
        }

        private void AddNamedComponentToList(IXSNamedComponent value)
        {
            if (value is XSElementDeclaration)
                ElementDeclarations.Add(value);

            else if (value is IXSType)
                TypeDefinitions.Add(value);

            else if (value is XSAttributeDeclaration)
                AttributeDeclarations.Add(value);

            else if (value is XSAttributeGroupDefinition)
                AttributeGroupDefinitions.Add(value);

            else if (value is XSNotationDeclaration)
                NotationDeclarations.Add(value);

            else if (value is XSIdentityConstraintDefinition)
                IdentityConstraintDefinitions.Add(value);

            else if (value is XSModelGroupDefinition)
                ModelGroupDefinitions.Add(value);
        }

        #endregion

        internal static string XMLStringIValue(IValue value)
        {
            switch (value.DataType)
            {
                case DataType.Undefined:
                    return "";

                case DataType.String:
                    return value.AsString();

                case DataType.Boolean:
                    return XmlConvert.ToString(value.AsBoolean());

                case DataType.Date:
                    return XmlConvert.ToString(value.AsDate(), XmlDateTimeSerializationMode.Unspecified);

                case DataType.Number:
                    return XmlConvert.ToString(value.AsNumber());

                default:
                    throw RuntimeException.InvalidArgumentType();
            }
        }
    }

    public sealed class XMLSchemaImpl_Serialize
    {
        public static XMLSchema ReadXML(XmlReaderImpl xmlReader)
        {
            XmlSchema xmlSchema = XmlSchema.Read(xmlReader.GetNativeReader(), null);
            return new XMLSchema(xmlSchema);
        }

        public static void WriteXML(XmlWriterImpl xmlWriter, XMLSchema schema)
        {
            XmlSchema xmlSchema = schema.SchemaObject;
            xmlSchema.Write(xmlWriter.GetNativeWriter());
        }
    }
}
