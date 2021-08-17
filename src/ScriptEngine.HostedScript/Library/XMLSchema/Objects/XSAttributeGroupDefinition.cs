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
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("ОпределениеГруппыАтрибутовXS", "XSAttributeGroupDefinition")]
    public class XSAttributeGroupDefinition : AutoContext<XSAttributeGroupDefinition>, IXSAnnotated, IXSNamedComponent
    {
        private XmlSchemaAnnotated _attributeGroup;
        private XSAnnotation _annotation;
        private XMLExpandedName _reference;
        private XSWildcard _wildcard;
        private string _name;

        private XSAttributeGroupDefinition()
        {
            _attributeGroup = new XmlSchemaAttributeGroup();

            Components = new XSComponentFixedList();
            Content = new XSComponentList();
            Content.Inserted += Content_Inserted;
            Content.Cleared += Content_Cleared;
        }

        internal XSAttributeGroupDefinition(XmlSchemaAttributeGroup xmlAttributeGroup)
            : this()
        {
            _attributeGroup = xmlAttributeGroup;
            _name = xmlAttributeGroup.Name;

            if (_attributeGroup.Annotation is XmlSchemaAnnotation annotation)
            {
                _annotation = XMLSchemaSerializer.CreateXSAnnotation(annotation);
                _annotation.BindToContainer(RootContainer, this);
            }

            if (xmlAttributeGroup.AnyAttribute is XmlSchemaAnyAttribute xmlAnyAttribute)
                _wildcard = XMLSchemaSerializer.CreateXSWildcard(xmlAnyAttribute);

            Content.Inserted -= Content_Inserted;
            foreach (XmlSchemaObject item in xmlAttributeGroup.Attributes)
            {
                IXSComponent component = XMLSchemaSerializer.CreateInstance(item);
                component.BindToContainer(RootContainer, this);
                Content.Add(component);
                Components.Add(component);
            }
            Content.Inserted += Content_Inserted;
        }

        internal XSAttributeGroupDefinition(XmlSchemaAttributeGroupRef xmlAttributeGroupRef)
            : this()
        {
            _attributeGroup = xmlAttributeGroupRef;

            if (_attributeGroup.Annotation is XmlSchemaAnnotation annotation)
            {
                _annotation = XMLSchemaSerializer.CreateXSAnnotation(annotation);
                _annotation.BindToContainer(RootContainer, this);
            }

            if (xmlAttributeGroupRef.RefName is XmlQualifiedName qualifiedName)
                _reference = XMLSchemaSerializer.CreateXMLExpandedName(qualifiedName);
        }

        private void OnSetAnnotation() => XSAnnotation.SetComponentAnnotation(_annotation, _attributeGroup);

        private void OnSetWildcard()
        {
            XmlSchemaAnyAttribute anyAttribute = _wildcard?.SchemaObject as XmlSchemaAnyAttribute;
            if (_attributeGroup is XmlSchemaAttributeGroup attributeGroup)
                attributeGroup.AnyAttribute = anyAttribute;
        }

        private void OnSetName()
        {
            if (_attributeGroup is XmlSchemaAttributeGroup attributeGroup)
                attributeGroup.Name = _name;
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
                OnSetAnnotation();
            }
        }

        [ContextProperty("Компоненты", "Components")]
        public XSComponentFixedList Components { get; }

        [ContextProperty("Контейнер", "Container")]
        public IXSComponent Container { get; private set; }

        [ContextProperty("КорневойКонтейнер", "RootContainer")]
        public IXSComponent RootContainer { get; private set; }

        [ContextProperty("Схема", "Schema")]
        public XMLSchema Schema => RootContainer?.Schema;

        [ContextProperty("ТипКомпоненты", "ComponentType")]
        public XSComponentType ComponentType => XSComponentType.AttributeGroupDefinition;

        [ContextProperty("URIПространстваИмен", "NamespaceURI")]
        public string NamespaceURI => _attributeGroup?.SourceUri;

        [ContextProperty("Имя", "Name")]
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnSetName();
            }
        }

        [ContextProperty("Маска", "Wildcard")]
        public XSWildcard Wildcard
        {
            get => _wildcard;
            set
            {
                _wildcard = value;
                _wildcard?.BindToContainer(RootContainer, this);
                OnSetWildcard();
            }
        }

        [ContextProperty("Содержимое", "Content")]
        public XSComponentList Content { get; }

        [ContextProperty("Ссылка", "Reference")]
        public XMLExpandedName Reference
        {
            get => _reference;
            set
            {
                _reference = value;
                if (_reference != null)
                {
                    _attributeGroup = new XmlSchemaAttributeGroupRef
                    {
                        RefName = _reference.NativeValue
                    };
                    Content.Clear();
                }
                else
                {
                    _attributeGroup = new XmlSchemaAttributeGroup();
                    OnSetWildcard();
                    OnSetName();
                }
                OnSetAnnotation();
            }
        }

        [ContextProperty("ЭтоСсылка", "IsReference")]
        public bool IsReference => _attributeGroup is XmlSchemaAttributeGroupRef;

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

        [ContextMethod("РазрешитьСсылку", "ResolveReference")]
        public XSAttributeGroupDefinition ResolveReference() => throw new NotImplementedException();

        #endregion

        #region Constructors

        [ScriptConstructor(Name = "По умолчанию")]
        public static XSAttributeGroupDefinition Constructor() => new XSAttributeGroupDefinition();

        #endregion

        #endregion

        #region IXSComponent

        public XmlSchemaObject SchemaObject => _attributeGroup;

        public void BindToContainer(IXSComponent rootContainer, IXSComponent container)
        {
            RootContainer = rootContainer;
            Container = container;
        }

        #endregion

        #region XSComponentListEvents

        private void Content_Inserted(object sender, XSComponentListEventArgs e)
        {
            IXSComponent component = e.Component;

            component.BindToContainer(RootContainer, this);
            Components.Add(component);
            if (_attributeGroup is XmlSchemaAttributeGroup attributeGroup)
                attributeGroup.Attributes.Add(component.SchemaObject);
        }

        private void Content_Cleared(object sender, EventArgs e)
        {
            Components.Clear();
            if (_attributeGroup is XmlSchemaAttributeGroup attributeGroup)
                attributeGroup.Attributes.Clear();
        }

        #endregion
    }
}
