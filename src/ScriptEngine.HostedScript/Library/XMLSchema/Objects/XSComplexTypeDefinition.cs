/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Xml;
using System.Xml.Schema;
using ScriptEngine.HostedScript.Library.Xml;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("ОпределениеСоставногоТипаXS", "XSComplexTypeDefinition")]
    class XSComplexTypeDefinition : AutoContext<XSComplexTypeDefinition>, IXSType, IXSNamedComponent
    {
        private readonly XmlSchemaComplexType _type;
        private XSAnnotation _annotation;
        private XSAnnotation _contentModelAnnotation;
        private XSAnnotation _derivationAnnotation;
        private XMLExpandedName _baseTypeName;
        private IXSComponent _content;
        private XSWildcard _attributeWildcard;
        private XSDerivationMethod _derivationMethod;
        private XSContentModel _contentModel;

        private XSComplexTypeDefinition()
        {
            _type = new XmlSchemaComplexType();
            Components = new XSComponentFixedList();
            Attributes = new XSComponentList();
            Attributes.Inserted += Attributes_Inserted;
            Attributes.Cleared += Attributes_Cleared;
        }

        private void OnSetContentModelDerivation()
        {
            if (_contentModel == XSContentModel.Simple)
            {
                _type.ContentModel = new XmlSchemaSimpleContent();

                if (_derivationMethod == XSDerivationMethod.Extension)
                    _type.ContentModel.Content = new XmlSchemaSimpleContentExtension();

                else if (_derivationMethod == XSDerivationMethod.Restriction)
                    _type.ContentModel.Content = new XmlSchemaSimpleContentRestriction();

                else
                    _type.ContentModel.Content = default(XmlSchemaContent);
            }
            else if (_contentModel == XSContentModel.Complex)
            {
                _type.ContentModel = new XmlSchemaComplexContent();

                if (_derivationMethod == XSDerivationMethod.Extension)
                    _type.ContentModel.Content = new XmlSchemaComplexContentExtension();

                else if (_derivationMethod == XSDerivationMethod.Restriction)
                    _type.ContentModel.Content = new XmlSchemaComplexContentRestriction();

                else
                    _type.ContentModel.Content = default(XmlSchemaContent);
            }
            else
                _type.ContentModel = default(XmlSchemaContentModel);

            OnSetBaseTypeName();
            OnSetAttributeWildcard();
            OnSetContentModelAnnotation();
            OnSetDerivationAnnotation();
        }

        private void OnSetBaseTypeName()
        {
            XmlQualifiedName baseTypeName = _baseTypeName?.NativeValue;

            if (_type.ContentModel is XmlSchemaComplexContent complexModel)
            {
                if (complexModel.Content is XmlSchemaComplexContentExtension contentExtension)
                    contentExtension.BaseTypeName = baseTypeName;

                else if (complexModel.Content is XmlSchemaComplexContentRestriction contentRestriction)
                    contentRestriction.BaseTypeName = baseTypeName;
            }
            else if (_type.ContentModel is XmlSchemaSimpleContent simpleModel)
            {
                if (simpleModel.Content is XmlSchemaSimpleContentExtension contentExtension)
                    contentExtension.BaseTypeName = baseTypeName;

                else if (simpleModel.Content is XmlSchemaSimpleContentRestriction contentRestriction)
                    contentRestriction.BaseTypeName = baseTypeName;
            }
        }

        private void OnSetAttributeWildcard()
        {

            XmlSchemaAnyAttribute anyAttribute = _attributeWildcard?.SchemaObject as XmlSchemaAnyAttribute;

            if (_type.ContentModel is XmlSchemaComplexContent complexModel)
            {
                if (complexModel.Content is XmlSchemaComplexContentExtension contentExtension)
                    contentExtension.AnyAttribute = anyAttribute;

                else if (complexModel.Content is XmlSchemaComplexContentRestriction contentRestriction)
                    contentRestriction.AnyAttribute = anyAttribute;
            }
            else if (_type.ContentModel is XmlSchemaSimpleContent simpleModel)
            {
                if (simpleModel.Content is XmlSchemaSimpleContentExtension contentExtension)
                    contentExtension.AnyAttribute = anyAttribute;

                else if (simpleModel.Content is XmlSchemaSimpleContentRestriction contentRestriction)
                    contentRestriction.AnyAttribute = anyAttribute;
            }
        }

        private void OnSetContentModelAnnotation()
        {
            XmlSchemaAnnotation annotation = _contentModelAnnotation?.SchemaObject;

            if (_type.ContentModel is XmlSchemaComplexContent complexModel)
                complexModel.Annotation = annotation;

            else if (_type.ContentModel is XmlSchemaSimpleContent simpleModel)
                simpleModel.Annotation = annotation;
        }

        private void OnSetDerivationAnnotation()
        {
            XmlSchemaAnnotation annotation = _derivationAnnotation?.SchemaObject;

            if (_type.ContentModel is XmlSchemaComplexContent complexModel)
            {
                if (complexModel.Content is XmlSchemaComplexContentExtension contentExtension)
                    contentExtension.Annotation = annotation;

                else if (complexModel.Content is XmlSchemaComplexContentRestriction contentRestriction)
                    contentRestriction.Annotation = annotation;
            }
            else if (_type.ContentModel is XmlSchemaSimpleContent simpleModel)
            {
                if (simpleModel.Content is XmlSchemaSimpleContentExtension contentExtension)
                    contentExtension.Annotation = annotation;

                else if (simpleModel.Content is XmlSchemaSimpleContentRestriction contentRestriction)
                    contentRestriction.Annotation = annotation;
            }
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
                _type.Annotation = _annotation?.SchemaObject;
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
        public XSAnnotation ContentModelAnnotation
        {
            get => _contentModelAnnotation;
            set
            {
                _contentModelAnnotation = value;
                _contentModelAnnotation?.BindToContainer(RootContainer, this);
                OnSetContentModelAnnotation();
            }
        }

        [ContextProperty("АннотацияНаследования", "DerivationAnnotation")]
        public XSAnnotation DerivationAnnotation
        {
            get => _derivationAnnotation;
            set
            {
                _derivationAnnotation = value;
                _derivationAnnotation?.BindToContainer(RootContainer, this);
                OnSetDerivationAnnotation();
            }
        }

        [ContextProperty("Атрибуты", "Attributes")]
        public XSComponentList Attributes { get; }

        //Блокировка(Block)
        //Завершенность(Final)
        //ЗапрещенныеПодстановки(ProhibitedSubstitutions)

        [ContextProperty("ИмяБазовогоТипа", "BaseTypeName")]
        public XMLExpandedName BaseTypeName
        {
            get => _baseTypeName;
            set
            {
                _baseTypeName = value;
                OnSetBaseTypeName();
            }
        }

        [ContextProperty("МаскаАтрибутов", "AttributeWildcard")]
        public XSWildcard AttributeWildcard
        {
            get => _attributeWildcard;
            set
            {
                _attributeWildcard = value;
                _attributeWildcard?.BindToContainer(RootContainer, this);
                OnSetAttributeWildcard();
            }
        }

        [ContextProperty("МетодНаследования", "DerivationMethod")]
        public XSDerivationMethod DerivationMethod
        {
            get => _derivationMethod;
            set
            {
                _derivationMethod = value;
                OnSetContentModelDerivation();
            }
        }

        [ContextProperty("МодельСодержимого", "ContentModel")]
        public XSContentModel ContentModel
        {
            get => _contentModel;
            set
            {
                _contentModel = value;
                OnSetContentModelDerivation();
            }
        }

        //ОпределениеБазовогоТипа(BaseTypeDefinition)

        [ContextProperty("Смешанный", "Mixed")]
        public bool Mixed => _type.ContentModel is XmlSchemaComplexContent complexContent ? complexContent.IsMixed : false;

        [ContextProperty("Содержимое", "Content")]
        public IXSComponent Content
        {
            get => _content;
            set
            {
                _content = value;
                _content.BindToContainer(RootContainer, this);
                if (_content is XSParticle particle)
                    _type.Particle = particle.SchemaObject;

                else if (_content is IXSFragment fragment)
                    _type.Particle = fragment.SchemaObject as XmlSchemaParticle;
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

        #region XSComponentListEvents

        private void Attributes_Inserted(object sender, XSComponentListEventArgs e)
        {
            var component = e.Component;
            if (!(component is IXSAttribute))
                throw RuntimeException.InvalidArgumentType();

            component.BindToContainer(RootContainer, this);
            Components.Add(component);

            _type.Attributes.Add(component.SchemaObject);
        }

        private void Attributes_Cleared(object sender, EventArgs e)
        {
            Components.RemoveAll(x => (x is IXSAttribute));

            _type.Attributes.Clear();
        }

        #endregion
    }
}