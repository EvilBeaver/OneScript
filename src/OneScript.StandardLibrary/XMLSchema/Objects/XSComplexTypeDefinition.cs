/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Xml;
using System.Xml.Schema;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.StandardLibrary.Xml;
using OneScript.StandardLibrary.XMLSchema.Collections;
using OneScript.StandardLibrary.XMLSchema.Enumerations;
using OneScript.StandardLibrary.XMLSchema.Interfaces;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.XMLSchema.Objects
{
    [ContextClass("ОпределениеСоставногоТипаXS", "XSComplexTypeDefinition")]
    public class XSComplexTypeDefinition : AutoContext<XSComplexTypeDefinition>, IXSType, IXSNamedComponent
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

        internal XSComplexTypeDefinition(XmlSchemaComplexType complexType)
            : this()
        {
            _type = complexType;

            if (_type.Annotation is XmlSchemaAnnotation annotation)
            {
                _annotation = XMLSchemaSerializer.CreateXSAnnotation(annotation);
                _annotation.BindToContainer(RootContainer, this);
            }

            if (_type.ContentModel is XmlSchemaSimpleContent simpleContent)
            {
                _contentModel = XSContentModel.Simple;
                if (simpleContent.Content is XmlSchemaSimpleContentExtension contentExtension)
                {
                    _derivationMethod = XSDerivationMethod.Extension;
                    if (contentExtension.BaseTypeName is XmlQualifiedName qualifiedName)
                        _baseTypeName = XMLSchemaSerializer.CreateXMLExpandedName(qualifiedName);

                    if (contentExtension.AnyAttribute is XmlSchemaAnyAttribute anyAttribute)
                        _attributeWildcard = XMLSchemaSerializer.CreateXSWildcard(anyAttribute);
                }
                else if (simpleContent.Content is XmlSchemaSimpleContentRestriction contentRestriction)
                {
                    _derivationMethod = XSDerivationMethod.Restriction;
                    if (contentRestriction.BaseTypeName is XmlQualifiedName qualifiedName)
                        _baseTypeName = XMLSchemaSerializer.CreateXMLExpandedName(qualifiedName);

                    if (contentRestriction.AnyAttribute is XmlSchemaAnyAttribute anyAttribute)
                        _attributeWildcard = XMLSchemaSerializer.CreateXSWildcard(anyAttribute);
                }
                else
                    _derivationMethod = XSDerivationMethod.EmptyRef;

                if (_type.Particle is XmlSchemaParticle particle)
                    _content = XMLSchemaSerializer.CreateInstance(particle);
            }
            else if (_type.ContentModel is XmlSchemaComplexContent complexContent)
            {
                _contentModel = XSContentModel.Complex;

                if (complexContent.Content is XmlSchemaComplexContentExtension contentExtension)
                {
                    _derivationMethod = XSDerivationMethod.Extension;
                    if (contentExtension.BaseTypeName is XmlQualifiedName qualifiedName)
                        _baseTypeName = XMLSchemaSerializer.CreateXMLExpandedName(qualifiedName);

                    if (contentExtension.Particle is XmlSchemaParticle particle)
                        _content = XMLSchemaSerializer.CreateInstance(particle);

                    if (contentExtension.AnyAttribute is XmlSchemaAnyAttribute anyAttribute)
                        _attributeWildcard = XMLSchemaSerializer.CreateXSWildcard(anyAttribute);
                }
                else if (complexContent.Content is XmlSchemaComplexContentRestriction contentRestriction)
                {
                    _derivationMethod = XSDerivationMethod.Restriction;
                    if (contentRestriction.BaseTypeName is XmlQualifiedName qualifiedName)
                        _baseTypeName = XMLSchemaSerializer.CreateXMLExpandedName(qualifiedName);

                    if (contentRestriction.Particle is XmlSchemaParticle particle)
                        _content = XMLSchemaSerializer.CreateInstance(particle);

                    if (contentRestriction.AnyAttribute is XmlSchemaAnyAttribute anyAttribute)
                        _attributeWildcard = XMLSchemaSerializer.CreateXSWildcard(anyAttribute);
                }
                else
                {
                    _derivationMethod = XSDerivationMethod.EmptyRef;

                    if (_type.Particle is XmlSchemaParticle particle)
                        _content = XMLSchemaSerializer.CreateInstance(particle);
                }
            }
            else
            {
                _contentModel = XSContentModel.EmptyRef;
                
                if (_type.Particle is XmlSchemaParticle particle)
                    _content = XMLSchemaSerializer.CreateInstance(particle);
            }
            
            Attributes.Inserted -= Attributes_Inserted;
            foreach (XmlSchemaObject item in _type.Attributes)
            {
                IXSComponent component = XMLSchemaSerializer.CreateInstance(item);
                component.BindToContainer(RootContainer, this);
                Attributes.Add(component);
                Components.Add(component);
            }
            Attributes.Inserted += Attributes_Inserted;
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
            if (_type.ContentModel is XmlSchemaComplexContent complexModel)
                XSAnnotation.SetComponentAnnotation(_contentModelAnnotation, complexModel);

            else if (_type.ContentModel is XmlSchemaSimpleContent simpleModel)
                XSAnnotation.SetComponentAnnotation(_contentModelAnnotation, simpleModel);
        }

        private void OnSetDerivationAnnotation()
        {
            if (_type.ContentModel is XmlSchemaComplexContent complexModel)
            {
                if (complexModel.Content is XmlSchemaComplexContentExtension contentExtension)
                    XSAnnotation.SetComponentAnnotation(_derivationAnnotation, contentExtension);

                else if (complexModel.Content is XmlSchemaComplexContentRestriction contentRestriction)
                    XSAnnotation.SetComponentAnnotation(_derivationAnnotation, contentRestriction);
            }
            else if (_type.ContentModel is XmlSchemaSimpleContent simpleModel)
            {
                if (simpleModel.Content is XmlSchemaSimpleContentExtension contentExtension)
                    XSAnnotation.SetComponentAnnotation(_derivationAnnotation, contentExtension);

                else if (simpleModel.Content is XmlSchemaSimpleContentRestriction contentRestriction)
                    XSAnnotation.SetComponentAnnotation(_derivationAnnotation, contentRestriction);
            }
        }

        private void OnSetContent()
        {
            XmlSchemaParticle xmlParticle;

            if (_content is XSParticle particle)
                xmlParticle = particle.SchemaObject;

            else if (_content is IXSFragment fragment)
                xmlParticle = fragment.SchemaObject as XmlSchemaParticle;

            else if (_content is XSModelGroupDefinition groupDefinition)
                xmlParticle = groupDefinition.SchemaObject as XmlSchemaGroupRef;

            else if (_content is XSModelGroup group)
                xmlParticle = group.SchemaObject;

            else
                xmlParticle = null;

            if (_type.ContentModel is XmlSchemaComplexContent complexModel)
            {
                if (complexModel.Content is XmlSchemaComplexContentExtension contentExtension)
                    contentExtension.Particle = xmlParticle;

                else if (complexModel.Content is XmlSchemaComplexContentRestriction contentRestriction)
                    contentRestriction.Particle = xmlParticle;
            }
            else
                _type.Particle = xmlParticle;
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

        [ContextProperty("Блокировка", "Block")]
        public XSProhibitedSubstitutionsUnion Block { get; }

        [ContextProperty("Завершенность", "Final")]
        public XSComplexFinalUnion Final { get; }

        [ContextProperty("ЗапрещенныеПодстановки", "ProhibitedSubstitutions")]
        public XSProhibitedSubstitutionsUnion ProhibitedSubstitutions { get; }

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
                OnSetContent();
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

        public void BindToContainer(IXSComponent rootContainer, IXSComponent container)
        {
            RootContainer = rootContainer;
            Container = container;
        }

        XmlSchemaObject IXSComponent.SchemaObject => _type;
        public XmlSchemaComplexType SchemaObject => _type;

        #endregion

        #region XSComponentListEvents

        private void Attributes_Inserted(object sender, XSComponentListEventArgs e)
        {
            IXSComponent component = e.Component;
            if (!(component is IXSAttribute) && (!(component is XSAttributeGroupDefinition)))
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
