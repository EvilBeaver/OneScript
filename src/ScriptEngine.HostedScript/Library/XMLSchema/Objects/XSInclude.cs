using System;
using System.Xml.Schema;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("ВключениеXS", "XSInclude")]
    public class XSInclude : AutoContext<XSInclude>, IXSDirective
    {

        private readonly XmlSchemaInclude _include;
        private IXSComponent _container;
        private IXSComponent _rootContainer;

        private XSInclude() => _include = new XmlSchemaInclude();

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
        public XSComponentType ComponentType => XSComponentType.Include;

        [ContextProperty("РазрешеннаяСхема", "ResolvedSchema")]
        public XMLSchema ResolvedSchema
        {
            get => ResolvedSchema;
            set => ResolvedSchema = value;
        }

        [ContextProperty("РасположениеСхемы", "SchemaLocation")]
        public string SchemaLocation
        {
            get => _include.SchemaLocation;
            set => _include.SchemaLocation = value;
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
        public static XSInclude Constructor() => new XSInclude();

        #endregion

        #endregion

        #region IXSComponent

        XmlSchemaObject IXSComponent.SchemaObject => _include;

        void IXSComponent.BindToContainer(IXSComponent rootContainer, IXSComponent container)
        {
            _rootContainer = rootContainer;
            _container = container;
        }

        #endregion

    }
}
