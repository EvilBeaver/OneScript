using System;
using System.Xml.Schema;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.HostedScript.Library.Xml;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("ОпределениеПростогоТипаXS", "XSSimpleTypeDefinition")]
    public class XSSimpleTypeDefinition : AutoContext<XSSimpleTypeDefinition>, IXSType, IXSNamedComponent
    {
        private readonly XmlSchemaSimpleType _type;
        private XSAnnotation _annotation;
        private XMLExpandedName _baseTypeName;
        private XMLExpandedName _itemTypeName;
        private XSSimpleTypeVariety _variety;

        private XSSimpleTypeDefinition()
        {
            _type = new XmlSchemaSimpleType();
            Facets = new XSComponentList();
            Facets.Inserted += Facets_Inserted;
            Facets.Cleared += Facets_Cleared;

            MemberTypeDefinitions = new XSComponentList();
            MemberTypeDefinitions.Inserted += MemberTypeDefinitions_Inserted;
            MemberTypeDefinitions.Cleared += MemberTypeDefinitions_Cleared;

            Components = new XSComponentFixedList();
            Variety = XSSimpleTypeVariety.Atomic;
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
        public XSComponentType ComponentType => XSComponentType.SimpleTypeDefinition;

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
        public XSSimpleTypeDefinition BaseType { get; }

        [ContextProperty("КорневойТип", "RootType")]
        public XSSimpleTypeDefinition RootType { get; }

        [ContextProperty("АннотацияВарианта", "VarietyAnnotation")]
        public XSAnnotation VarietyAnnotation { get; set; }

        [ContextProperty("Вариант", "Variety")]
        public XSSimpleTypeVariety Variety
        {
            get => _variety;
            set
            {
                _variety = value;

                if (_variety == XSSimpleTypeVariety.List)
                    _type.Content = new XmlSchemaSimpleTypeList();

                else if (_variety == XSSimpleTypeVariety.Union)
                    _type.Content = new XmlSchemaSimpleTypeUnion();

                else
                    _type.Content = new XmlSchemaSimpleTypeRestriction();
            }
        }

        [ContextProperty("Завершенность", "Final")]
        public XSSimpleFinalUnion Final { get; }

        [ContextProperty("ИменаТиповОбъединения", "MemberTypeNames")]
        public XMLExpandedNameList MemberTypeNames { get; }

        [ContextProperty("ИмяБазовогоТипа", "BaseTypeName")]
        public XMLExpandedName BaseTypeName
        {
            get => _baseTypeName;
            set
            {
                _baseTypeName = value;
                if (Variety == XSSimpleTypeVariety.Atomic)
                {
                    XmlSchemaSimpleTypeRestriction __content = _type.Content as XmlSchemaSimpleTypeRestriction;
                    __content.BaseTypeName = _baseTypeName.NativeValue;
                }
                else
                    throw RuntimeException.InvalidArgumentValue();
            }
        }

        [ContextProperty("ИмяТипаЭлемента", "ItemTypeName")]
        public XMLExpandedName ItemTypeName
        {
            get => _itemTypeName;
            set
            {
                _itemTypeName = value;
                if (Variety == XSSimpleTypeVariety.List)
                {
                    XmlSchemaSimpleTypeList __content = _type.Content as XmlSchemaSimpleTypeList;
                    __content.ItemTypeName = _itemTypeName.NativeValue;
                }
            }
        }

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
        public IXSComponent CloneComponent(bool recursive = true) => throw new NotImplementedException();

        [ContextMethod("ОбновитьЭлементDOM", "UpdateDOMElement")]
        public void UpdateDOMElement() => throw new NotImplementedException();

        [ContextMethod("Содержит", "Contains")]
        public bool Contains(IXSComponent component) => Components.Contains(component);

        [ContextMethod("ЭтоОпределениеЗациклено", "IsCircular")]
        public bool IsCircular() => throw new NotImplementedException();
       
        #endregion

        #region Constructors

        [ScriptConstructor(Name = "По умолчанию")]
        public static XSSimpleTypeDefinition Constructor() => new XSSimpleTypeDefinition();

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

        #region XSComponentListEvents

        private void Facets_Inserted(object sender, XSComponentListEventArgs e)
        {
            var component = e.Component;

            if (!(component is IXSFacet))
                throw RuntimeException.InvalidArgumentType();

            component.BindToContainer(RootContainer, this);
            Components.Add(component);

            if (_type.Content is XmlSchemaSimpleTypeRestriction content)
                content.Facets.Add(component.SchemaObject);
        }

        private void Facets_Cleared(object sender, EventArgs e)
        {
            Components.Clear();

            if (_type.Content is XmlSchemaSimpleTypeRestriction content)
                content.Facets.Clear();
        }

        private void MemberTypeDefinitions_Inserted(object sender, XSComponentListEventArgs e)
        {
            var component = e.Component;

            if (!(component is XSSimpleTypeDefinition))
                throw RuntimeException.InvalidArgumentType();

            component.BindToContainer(RootContainer, this);
            Components.Add(component);

            if (_type.Content is XmlSchemaSimpleTypeUnion content)
                content.BaseTypes.Add(component.SchemaObject);
        }

        private void MemberTypeDefinitions_Cleared(object sender, EventArgs e)
        {
            Components.Clear();

            if (_type.Content is XmlSchemaSimpleTypeUnion content)
                content.BaseTypes.Clear();
        }

        #endregion
    }
}