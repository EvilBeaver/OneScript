using System;
using System.Xml.Schema;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("ПереопределениеXS", "XSRedefine")]
    public class XSRedefine : AutoContext<XSRedefine>, IXSDirective
    {

        private readonly XmlSchemaRedefine _redefine;

        private XSRedefine()
        {
            _redefine = new XmlSchemaRedefine();
            Components = new XSComponentFixedList();

            Content = new XSComponentList();
            Content.Inserted += Content_Inserted;
            Content.Cleared += Content_Cleared;
        }

        #region OneScript

        #region Properties

        [ContextProperty("Аннотация", "Annotation")]
        public XSAnnotation Annotation => null;

        [ContextProperty("Компоненты", "Components")]
        public XSComponentFixedList Components { get; }

        [ContextProperty("Контейнер", "Container")]
        public IXSComponent Container { get; private set; }

        [ContextProperty("КорневойКонтейнер", "RootContainer")]
        public IXSComponent RootContainer { get; private set; }

        [ContextProperty("Схема", "Schema")]
        public XMLSchema Schema => RootContainer.Schema;

        [ContextProperty("ТипКомпоненты", "ComponentType")]
        public XSComponentType ComponentType => XSComponentType.Redefine;

        [ContextProperty("РазрешеннаяСхема", "ResolvedSchema")]
        public XMLSchema ResolvedSchema
        {
            get => ResolvedSchema;
            set => ResolvedSchema = value;
        }

        [ContextProperty("РасположениеСхемы", "SchemaLocation")]
        public string SchemaLocation
        {
            get => _redefine.SchemaLocation;
            set => _redefine.SchemaLocation = value;
        }

        [ContextProperty("Содержимое", "Content")]
        public XSComponentList Content { get; }

        #endregion

        #region Methods

        [ContextMethod("КлонироватьКомпоненту", "CloneComponent")]
        public IXSComponent CloneComponent(bool recursive = true) => throw new NotImplementedException();

        [ContextMethod("ОбновитьЭлементDOM", "UpdateDOMElement")]
        public void UpdateDOMElement() => throw new NotImplementedException();

        [ContextMethod("Содержит", "Contains")]
        public bool Contains(IXSComponent component) => Components.Contains(component);

        #endregion

        #region Constructors

        [ScriptConstructor(Name = "По умолчанию")]
        public static XSRedefine Constructor() => new XSRedefine();

        #endregion

        #endregion

        #region IXSComponent

        void IXSComponent.BindToContainer(IXSComponent rootContainer, IXSComponent container)
        {
            RootContainer = rootContainer;
            Container = container;
        }

        XmlSchemaObject IXSComponent.SchemaObject => _redefine;

        #endregion

        #region XSComponentListEvents

        private void Content_Inserted(object sender, XSComponentListEventArgs e)
        {
            var component = e.Component;

            component.BindToContainer(RootContainer, this);
            Components.Add(component);
            _redefine.Items.Add(component.SchemaObject);
        }

        private void Content_Cleared(object sender, EventArgs e)
        {
            Components.Clear();
            _redefine.Items.Clear();
        }

        #endregion
    }
}
