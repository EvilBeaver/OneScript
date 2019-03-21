using System;
using System.Xml.Schema;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("ИнформацияДляПриложенияXS", "XSAppInfo")]
    public class XSAppInfo : AutoContext<XSAppInfo>, IXSComponent, IXSAnnotationItem
    {

        private readonly XmlSchemaAppInfo _appInfo;
        private IXSComponent _container;
        private IXSComponent _rootContainer;

        private XSAppInfo() => _appInfo = new XmlSchemaAppInfo();

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
        public XSComponentType ComponentType => XSComponentType.AppInfo;

        [ContextProperty("Источник", "Source")]
        public string Source
        {
            get => _appInfo.Source;
            set => _appInfo.Source = value;
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
        public static XSAppInfo Constructor() => new XSAppInfo();

        #endregion

        #endregion

        #region IXComponent

        XmlSchemaObject IXSComponent.SchemaObject => _appInfo;

        void IXSComponent.BindToContainer(IXSComponent rootContainer, IXSComponent container)
        {
            _rootContainer = rootContainer;
            _container = container;
        }

        #endregion
    }
}
