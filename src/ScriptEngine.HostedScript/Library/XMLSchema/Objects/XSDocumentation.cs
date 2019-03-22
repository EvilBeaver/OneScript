using System;
using System.Diagnostics.Contracts;
using System.Xml.Schema;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("ДокументацияXS", "XSDocumentation")]
    public class XSDocumentation : AutoContext<XSDocumentation>, IXSAnnotationItem
    {

        private readonly XmlSchemaDocumentation _documentation;
        private XSAnnotation _container;
        private IXSComponent _rootContainer;

        private XSDocumentation() => _documentation = new XmlSchemaDocumentation();

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
        public XSComponentType ComponentType => XSComponentType.Documentation;

        [ContextProperty("Источник", "Source")]
        public string Source
        {
            get => _documentation.Source;
            set => _documentation.Source = value;
        }

        [ContextProperty("Язык", "Language")]
        public string Language
        {
            get => _documentation.Language;
            set => _documentation.Language = value;
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
        public static XSDocumentation Constructor() => new XSDocumentation();

        #endregion

        #endregion

        #region IXSComponent

        XmlSchemaObject IXSComponent.SchemaObject => _documentation;

        void IXSComponent.BindToContainer(IXSComponent rootContainer, IXSComponent container)
        {
            Contract.Requires(container is XSAnnotation);
            _rootContainer = rootContainer;
            _container = (XSAnnotation)container;
        }

        #endregion

    }
}
