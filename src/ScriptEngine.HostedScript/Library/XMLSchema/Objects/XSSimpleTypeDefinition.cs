using System;
using System.Xml.Schema;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.HostedScript.Library.Xml;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("ОпределениеПростогоТипаXS", "XSSimpleTypeDefinition")]
    public class XSSimpleTypeDefinition : AutoContext<XSSimpleTypeDefinition>, IXSType, IXSNamedComponent, IXSListOwner
    {
        private readonly XmlSchemaSimpleType _type;
        private XSAnnotation _annotation;
        private IXSComponent _container;
        private IXSComponent _rootContainer;
        private XMLExpandedName _baseTypeName;

        private XSSimpleTypeDefinition()
        {
            _type = new XmlSchemaSimpleType();
            Facets = new XSComponentList(this);
        }

        #region OneScript

        #region Properties

        [ContextProperty("Аннотация", "Annotation")]
        public XSAnnotation Annotation
        {
            get => _annotation;
            set
            {
                _annotation = value;
                _type.Annotation = value.InternalObject;
            }
        }

        [ContextProperty("Компоненты", "Components")]
        public XSComponentFixedList Components { get; }

        [ContextProperty("Контейнер", "Container")]
        public IValue Container => _container;

        [ContextProperty("КорневойКонтейнер", "RootContainer")]
        public IValue RootContainer => _rootContainer;

        [ContextProperty("Схема", "Schema")]
        public XMLSchema Schema => _rootContainer.Schema;

        [ContextProperty("ТипКомпоненты", "ComponentType")]
        public XSComponentType ComponentType => XSComponentType.SimpleTypeDefinition;

        [ContextProperty("ЭлементDOM", "DOMElement")]
        public IValue DOMElement => null;

        [ContextProperty("URIПространстваИмен", "NamespaceURI")]
        public string URIПространстваИмен => _type.SourceUri;

        [ContextProperty("Имя", "Name")]
        public string Name
        {
            get => _type.Name;
            set => _type.Name = value;
        }
        
        [ContextProperty("БазовыйТип", "BaseType")]
        public XSSimpleTypeDefinition BaseType { get; }

        [ContextProperty("КорневойТип", "RootType")]
        public XSSimpleTypeDefinition RootType { get; }

        [ContextProperty("АннотацияВарианта", "VarietyAnnotation")]
        public XSAnnotation VarietyAnnotation { get; set; }

        [ContextProperty("Вариант", "Variety")]
        public XSSimpleTypeVariety Variety { get; set; }

        [ContextProperty("Завершенность", "Final")]
        public XSSimpleFinalUnion Final { get; }

        [ContextProperty("ИменаТиповОбъединения", "MemberTypeNames")]
        public XMLExpandedNameList MemberTypeNames { get; }

        [ContextProperty("ИмяБазовогоТипа", "BaseTypeName")]
        public XMLExpandedName BaseTypeName
        {
            get => _baseTypeName;
            set => _baseTypeName = value;
        }

        [ContextProperty("ИмяТипаЭлемента", "ItemTypeName")]
        public XMLExpandedName ItemTypeName { get; set; }

        [ContextProperty("ОпределениеБазовогоТипа", "BaseTypeDefinition")]
        public XSSimpleTypeDefinition BaseTypeDefinition { get; set; }

        [ContextProperty("ОпределениеПримитивногоТипа", "PrimitiveTypeDefinition")]
        public XSSimpleTypeDefinition PrimitiveTypeDefinition { get; set; }

        [ContextProperty("ОпределениеТипаЭлемента", "ItemTypeDefinition")]
        public XSSimpleTypeDefinition ItemTypeDefinition { get; set; }

        [ContextProperty("ОпределенияТиповОбъединения", "MemberTypeDefinitions")]
        public XSComponentList MemberTypeDefinitions { get; }
        
        [ContextProperty("Фасеты", "Facets")]
        public XSComponentList Facets { get; }

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
        public bool Contains(IValue component) => (component == this);

        [ContextMethod("ЭтоОпределениеЗациклено", "IsCircular")]
        public bool IsCircular()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Constructors

        [ScriptConstructor(Name = "По умолчанию")]
        public static XSSimpleTypeDefinition Constructor() => new XSSimpleTypeDefinition();

        #endregion
        
        #endregion

        #region IXSComponent

        void IXSComponent.BindToContainer(IXSComponent rootContainer, IXSComponent container)
        {
            _rootContainer = rootContainer;
            _container = container;
        }

        XmlSchemaObject IXSComponent.SchemaObject => _type;

        #endregion

        #region IXSListOwner

        void IXSListOwner.OnListInsert(XSComponentList List, IXSComponent component)
        {
            component.BindToContainer(this, this);
            Components.Add(component);
        }

        void IXSListOwner.OnListDelete(XSComponentList List, IXSComponent component) { }

        void IXSListOwner.OnListClear(XSComponentList List) { }

        #endregion
    }
}