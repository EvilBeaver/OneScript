using System;
using System.Xml.Schema;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{

    /// <summary>
    /// Описывает тег "import" для схемы XML.
    /// </summary>
    /// <see cref="System.Xml.Schema.XmlSchemaImport"/> 
    [ContextClass("ИмпортXS", "XSImport")]
    public class XSImport : AutoContext<XSImport>, IXSDirective
    {

        private readonly XmlSchemaImport _import;
        private IXSComponent _container;
        private IXSComponent _rootContainer;
      
        private XSImport() => _import = new XmlSchemaImport();

        #region OneScript

        #region Properties

        [ContextProperty("Аннотация", "Annotation")]
        public XSAnnotation Annotation => null;

        [ContextProperty("Компоненты", "Components")]
        public XSComponentFixedList Components => null;

        [ContextProperty("Контейнер", "Container")]
        public IValue Container => _container;

        [ContextProperty("КорневойКонтейнер", "RootContainer")]
        public IValue RootContainer => _rootContainer;

        [ContextProperty("Схема", "Schema")]
        public XMLSchema Schema => _rootContainer.Schema;

        [ContextProperty("ТипКомпоненты", "ComponentType")]
        public XSComponentType ComponentType => XSComponentType.Import;

        [ContextProperty("РазрешеннаяСхема", "ResolvedSchema")]
        public XMLSchema ResolvedSchema
        {
            get => ResolvedSchema;
            set => ResolvedSchema = value;
        }

        [ContextProperty("РасположениеСхемы", "SchemaLocation")]
        public string SchemaLocation
        {
            get => _import.SchemaLocation;
            set => _import.SchemaLocation = value;
        }

        [ContextProperty("Префикс", "Prefix")]
        public string Prefix { get; set; }

        [ContextProperty("ПространствоИмен", "Namespace")]
        public string Namespace
        {
            get => _import.Namespace;
            set => _import.Namespace = value;
        }

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
        public bool Contains(IValue component) => false;

        #endregion

        #region Constructors

        [ScriptConstructor(Name = "По умолчанию")]
        public static XSImport Constructor() => new XSImport();

        #endregion

        #endregion

        #region IXSComponent

        XmlSchemaObject IXSComponent.SchemaObject => _import;

        void IXSComponent.BindToContainer(IXSComponent rootContainer, IXSComponent container)
        {
            _rootContainer = rootContainer;
            _container = container;
        }

        #endregion

    }
}
