/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Diagnostics.Contracts;
using System.Xml;
using System.Xml.Schema;
using OneScript.Contexts;
using OneScript.StandardLibrary.Xml;
using OneScript.StandardLibrary.XMLSchema.Collections;
using OneScript.StandardLibrary.XMLSchema.Enumerations;
using OneScript.StandardLibrary.XMLSchema.Interfaces;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.XMLSchema.Objects
{
    [ContextClass("ОпределениеПростогоТипаXS", "XSSimpleTypeDefinition")]
    public sealed class XSSimpleTypeDefinition : AutoContext<XSSimpleTypeDefinition>, IXSType, IXSNamedComponent
    {
        private readonly XmlSchemaSimpleType _type;
        private readonly XSComponentFixedList _components = new XSComponentFixedList();
        private readonly XSComponentList _typeDefinitions = new XSComponentList();
        private readonly XSComponentList _facets = new XSComponentList();

        private XSAnnotation _annotation;
        private XMLExpandedName _baseTypeName;
        private XMLExpandedName _itemTypeName;
        private XSSimpleTypeVariety _variety;

        private XSSimpleTypeDefinition() : this(new XmlSchemaSimpleType()) { }

        internal XSSimpleTypeDefinition(XmlSchemaSimpleType simpleType)
        {
            _type = simpleType;

            if (_type.Annotation is XmlSchemaAnnotation annotation)
            {
                _annotation = XMLSchemaSerializer.CreateXSAnnotation(annotation);
                _annotation.BindToContainer(RootContainer, this);
            }

            InitContentTypeVariety();

            _facets.Inserted += Facets_Inserted;
            _facets.Cleared += Facets_Cleared;

            _typeDefinitions.Inserted += MemberTypeDefinitions_Inserted;
            _typeDefinitions.Cleared += MemberTypeDefinitions_Cleared;
        }

        private void InitContentTypeVariety()
        {
            if (_type.Content is XmlSchemaSimpleTypeList typeList)
                InitListVariety(typeList);

            else if (_type.Content is XmlSchemaSimpleTypeUnion typeUnion)
                InitUnionVariety(typeUnion);

            else if (_type.Content is XmlSchemaSimpleTypeRestriction typeRestriction)
                InitAtomicVariety(typeRestriction);

            else
            {
                var newRestriction = new XmlSchemaSimpleTypeRestriction();
                _type.Content = newRestriction;
                InitAtomicVariety(newRestriction);
            }
        }

        private void InitListVariety(XmlSchemaSimpleTypeList typeList)
        {
            _variety = XSSimpleTypeVariety.List;
            InitItemTypeName(typeList.ItemTypeName);
        }

        private void InitUnionVariety(XmlSchemaSimpleTypeUnion typeUnion)
        {
            _variety = XSSimpleTypeVariety.Union;
            foreach (var item in typeUnion.BaseTypes)
            {
                var component = XMLSchemaSerializer.CreateInstance(item);
                component.BindToContainer(RootContainer, this);
                _typeDefinitions.Add(component);
                _components.Add(component);
            }          
        }

        private void InitAtomicVariety(XmlSchemaSimpleTypeRestriction typeRestriction)
        {
            _variety = XSSimpleTypeVariety.Atomic;
            InitBaseTypeName(typeRestriction.BaseTypeName);

            foreach (var item in typeRestriction.Facets)
            {
                var component = XMLSchemaSerializer.CreateInstance(item);
                component.BindToContainer(RootContainer, this);
                _facets.Add(component);
                _components.Add(component);
            }
        }

        private void InitItemTypeName(XmlQualifiedName xmlQualifiedName)
        {
            if (xmlQualifiedName is XmlQualifiedName qualifiedName)
                _itemTypeName = new XMLExpandedName(qualifiedName);
        }

        private void InitBaseTypeName(XmlQualifiedName xmlQualifiedName)
        {
            if (xmlQualifiedName is XmlQualifiedName qualifiedName)
                _baseTypeName = new XMLExpandedName(qualifiedName);
        }

        private void SetContentTypeVariety(XSSimpleTypeVariety value)
        {
            if (_variety == value) return;
            _variety = value;

            switch (_variety)
            {
                case XSSimpleTypeVariety.List:
                    _type.Content = new XmlSchemaSimpleTypeList();
                    _itemTypeName = default;
                    break;

                case XSSimpleTypeVariety.Union:
                    _type.Content = new XmlSchemaSimpleTypeUnion();
                    _typeDefinitions.Clear();
                    break;

                case XSSimpleTypeVariety.Atomic:
                    _type.Content = new XmlSchemaSimpleTypeRestriction();
                    _baseTypeName = default;
                    _facets.Clear();
                    break;

                default:
                    break;
            }
        }

        private void SetBaseTypeName(XMLExpandedName value)
        {
            if (_baseTypeName == value) return;
            Contract.Requires(Variety == XSSimpleTypeVariety.Atomic);
            _baseTypeName = value;
            
             var content = _type.Content as XmlSchemaSimpleTypeRestriction;
             content.BaseTypeName = _baseTypeName?.NativeValue;
        }

        private void SetItemTypeName(XMLExpandedName value)
        {
            if (_itemTypeName == value) return;
            Contract.Requires(Variety == XSSimpleTypeVariety.List);
            _itemTypeName = value;

            var content = _type.Content as XmlSchemaSimpleTypeList;
            content.ItemTypeName = _itemTypeName?.NativeValue;
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
                _annotation?.BindToContainer(RootContainer, this);
                XSAnnotation.SetComponentAnnotation(_annotation, _type);
            }
        }

        [ContextProperty("Компоненты", "Components")]
        public XSComponentFixedList Components => _components;

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
        public string NamespaceURI => _type.SourceUri;

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
            set => SetContentTypeVariety(value);
        }

        [ContextProperty("Завершенность", "Final")]
        public XSSimpleFinalUnion Final { get; }

        [ContextProperty("ИменаТиповОбъединения", "MemberTypeNames")]
        public XMLExpandedNameList MemberTypeNames { get; }

        [ContextProperty("ИмяБазовогоТипа", "BaseTypeName")]
        public XMLExpandedName BaseTypeName
        {
            get => _baseTypeName;
            set => SetBaseTypeName(value);
        }

        [ContextProperty("ИмяТипаЭлемента", "ItemTypeName")]
        public XMLExpandedName ItemTypeName
        {
            get => _itemTypeName;
            set => SetItemTypeName(value);
        }

        [ContextProperty("ОпределениеБазовогоТипа", "BaseTypeDefinition")]
        public XSSimpleTypeDefinition BaseTypeDefinition { get; set; }

        [ContextProperty("ОпределениеПримитивногоТипа", "PrimitiveTypeDefinition")]
        public XSSimpleTypeDefinition PrimitiveTypeDefinition { get; set; }

        [ContextProperty("ОпределениеТипаЭлемента", "ItemTypeDefinition")]
        public XSSimpleTypeDefinition ItemTypeDefinition { get; set; }

        [ContextProperty("ОпределенияТиповОбъединения", "MemberTypeDefinitions")]
        public XSComponentList MemberTypeDefinitions => _typeDefinitions;

        [ContextProperty("Фасеты", "Facets")]
        public XSComponentList Facets => _facets;

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

        public void BindToContainer(IXSComponent rootContainer, IXSComponent container)
        {
            RootContainer = rootContainer;
            Container = container;
        }

        XmlSchemaObject IXSComponent.SchemaObject => _type;
        public XmlSchemaSimpleType SchemaObject => _type;

        #endregion

        #region XSComponentListEvents

        private void Facets_Inserted(object sender, XSComponentListEventArgs e)
        {
            var component = e.Component;
            Contract.Requires(_variety == XSSimpleTypeVariety.Atomic);
            Contract.Requires(component is IXSFacet);

            component.BindToContainer(RootContainer, this);
            _components.Add(component);

            var content = _type.Content as XmlSchemaSimpleTypeRestriction;
            content.Facets.Add(component.SchemaObject);
        }

        private void Facets_Cleared(object sender, EventArgs e)
        {
            Contract.Requires(_variety == XSSimpleTypeVariety.Atomic);
            _components.Clear();

            var content = _type.Content as XmlSchemaSimpleTypeRestriction;
            content.Facets.Clear();
        }

        private void MemberTypeDefinitions_Inserted(object sender, XSComponentListEventArgs e)
        {
            var component = e.Component;
            Contract.Requires(_variety == XSSimpleTypeVariety.Union);
            Contract.Requires(component is XSSimpleTypeDefinition);

            component.BindToContainer(RootContainer, this);
            _components.Add(component);

            var content = _type.Content as XmlSchemaSimpleTypeUnion;
            content.BaseTypes.Add(component.SchemaObject);
        }

        private void MemberTypeDefinitions_Cleared(object sender, EventArgs e)
        {
            Contract.Requires(_variety == XSSimpleTypeVariety.Union);
            _components.Clear();

            var content = _type.Content as XmlSchemaSimpleTypeUnion;
            content.BaseTypes.Clear();
        }

        #endregion
    }
}
