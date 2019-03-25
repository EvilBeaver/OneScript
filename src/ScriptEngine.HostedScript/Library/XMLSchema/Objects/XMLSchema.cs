using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("СхемаXML", "XMLSchema")]
    public class XMLSchema : AutoContext<XMLSchema>, IXSComponent, IXSListOwner
    {

        private readonly XmlSchema _schema;

        private XMLSchema()
        {
            _schema = new XmlSchema();
            Components = new XSComponentFixedList();
            Annotations = new XSComponentFixedList();
            Directives = new XSComponentList(this);
            Content = new XSComponentList(this);
            BlockDefault = new XSDisallowedSubstitutionsUnion();
            FinalDefault = new XSSchemaFinalUnion();
            AttributeGroupDefinitions = new XSNamedComponentMap();
            NotationDeclarations = new XSNamedComponentMap();
            ElementDeclarations = new XSNamedComponentMap();
            AttributeGroupDefinitions = new XSNamedComponentMap();
            ModelGroupDefinitions = new XSNamedComponentMap();
            IdentityConstraintDefinitions = new XSNamedComponentMap();
            TypeDefinitions = new XSNamedComponentMap();
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
        public IValue Container => this;

        [ContextProperty("КорневойКонтейнер", "RootContainer")]
        public IValue RootContainer => this;

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
        public string SchemaLocation { get; set;  }

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
        public IValue CloneComponent(IValue recursive = null)
        {
            throw new NotImplementedException();
        }

        [ContextMethod("ОбновитьЭлементDOM", "UpdateDOMElement")]
        public void UpdateDOMElement()
        {
            throw new NotImplementedException();
        }

        [ContextMethod("Содержит", "Contains")]
        public bool Contains(IValue component) => Components.Contains(component);

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

        void IXSComponent.BindToContainer(IXSComponent RootContainer, IXSComponent Container) { }

        #endregion

        #region IXSListOwner

        void IXSListOwner.OnListInsert(XSComponentList List, IXSComponent component)
        {
            component.BindToContainer(this, this);
            Components.Add(component);

            if (component is IXSDirective)
                _schema.Includes.Add(component.SchemaObject);
            else
                _schema.Items.Add(component.SchemaObject);

            if (component is IXSNamedComponent)
                if (component is XSElementDeclaration)
                    ElementDeclarations.Add((IXSNamedComponent)component);
        }

        void IXSListOwner.OnListDelete(XSComponentList List, IXSComponent component) { }

        void IXSListOwner.OnListClear(XSComponentList List) { }

        #endregion

    }
}
