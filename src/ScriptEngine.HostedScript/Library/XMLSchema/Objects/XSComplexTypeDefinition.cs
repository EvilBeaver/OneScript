using System;
using System.Xml.Schema;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.HostedScript.Library.Xml;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("ОпределениеСоставногоТипаXS", "XSComplexTypeDefinition")]
    class XSComplexTypeDefinition : AutoContext<XSComplexTypeDefinition>, IXSType, IXSNamedComponent
    {
        private readonly XmlSchemaComplexType _type;
        private XSAnnotation _annotation;
        private XMLExpandedName _baseTypeName;
        private XSParticle _particle;

        private XSComplexTypeDefinition()
        {
            _type = new XmlSchemaComplexType();
            Components = new XSComponentFixedList();
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
                (_annotation as IXSComponent).BindToContainer(RootContainer, this);
            }
        }

        [ContextProperty("Компоненты", "Components")]
        public XSComponentFixedList Components { get; }

        [ContextProperty("Контейнер", "Container")]
        public IXSComponent Container { get; private set; }

        [ContextProperty("КорневойКонтейнер", "RootContainer")]
        public IXSComponent RootContainer { get; private set; }

        [ContextProperty("Схема", "Schema")]
        public XMLSchema Schema => RootContainer.Schema;

        [ContextProperty("ТипКомпоненты", "ComponentType")]
        public XSComponentType ComponentType => XSComponentType.ComplexTypeDefinition;
        
        [ContextProperty("ЭлементDOM", "DOMElement")]
        public IValue DOMElement => ValueFactory.Create();

        [ContextProperty("URIПространстваИмен", "NamespaceURI")]
        public string URIПространстваИмен => _type.SourceUri;
        
        [ContextProperty("Имя", "Name")]
        public string Name
        {
            get => _type.Name;
            set => _type.Name = value;
        }

        [ContextProperty("БазовыйТип", "BaseType")]
        public XSComplexTypeDefinition BaseType { get; }

        [ContextProperty("КорневойТип", "RootType")]
        public XSComplexTypeDefinition RootType { get; }

        [ContextProperty("Абстрактный", "Abstract")]
        public bool Abstract
        {
            get => _type.IsAbstract;
            set => _type.IsAbstract = value;
        }

        [ContextProperty("АннотацияМоделиСодержимого", "ContentModelAnnotation")]
        public XSAnnotation ContentModelAnnotation { get; set; }

        [ContextProperty("АннотацияНаследования", "DerivationAnnotation")]
        public XSAnnotation DerivationAnnotation { get; set; }

        [ContextProperty("Атрибуты", "Attributes")]
        public XSComponentList Attributes { get; }

        //Блокировка(Block)
        //Завершенность(Final)
        //ЗапрещенныеПодстановки(ProhibitedSubstitutions)

        [ContextProperty("ИмяБазовогоТипа", "BaseTypeName")]
        public XMLExpandedName BaseTypeName
        {
            get => _baseTypeName;
            set => _baseTypeName = value;
        }

        //МаскаАтрибутов(AttributeWildcard)
        //МетодНаследования(DerivationMethod)
        //МодельСодержимого(ContentModel)
        //ОпределениеБазовогоТипа(BaseTypeDefinition)
        //Смешанный(Mixed)

        [ContextProperty("Содержимое", "Content")]
        public XSParticle Content
        {
            get => _particle;
            set
            {
                _particle = value;
                _type.Particle = (_particle as IXSComponent).SchemaObject as XmlSchemaParticle;
            }
        }

       //Фасеты(Facets)

        #endregion

        #region Methods

       [ContextMethod("КлонироватьКомпоненту", "CloneComponent")]
        public IXSComponent CloneComponent(bool recursive = true) => throw new NotImplementedException();

        [ContextMethod("ОбновитьЭлементDOM", "UpdateDOMElement")]
        public void UpdateDOMElement() => throw new NotImplementedException();

        [ContextMethod("Содержит", "Contains")]
        public bool Contains(IXSComponent component) => Components.Contains(component);

        [ContextMethod("ЭтоОпределениеЗациклено", "IsCircular")]
        public bool IsCircular() => throw new NotImplementedException();

        [ContextMethod("ИспользованиеАтрибутов", "AttributeUses")]
        public XSComponentFixedList AttributeUses() => throw new NotImplementedException();

        #endregion

        #region Constructors

        [ScriptConstructor(Name = "По умолчанию")]
        public static XSComplexTypeDefinition Constructor() => new XSComplexTypeDefinition();

        #endregion

        #endregion

        #region IXSComponent

        void IXSComponent.BindToContainer(IXSComponent rootContainer, IXSComponent container)
        {
            RootContainer = rootContainer;
            Container = container;
        }

        XmlSchemaObject IXSComponent.SchemaObject => _type;

        #endregion
    }
}
