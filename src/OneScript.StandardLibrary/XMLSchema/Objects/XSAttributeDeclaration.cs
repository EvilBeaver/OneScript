/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
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
    [ContextClass("ОбъявлениеАтрибутаXS", "XSAttributeDeclaration")]
    public class XSAttributeDeclaration : AutoContext<XSAttributeDeclaration>, IXSAnnotated, IXSAttribute, IXSNamedComponent
    {
        private readonly XmlSchemaAttribute _attribute;
        private XSAnnotation _annotation;
        private XSConstraint _constraint;
        private XMLExpandedName _refName;
        private XMLExpandedName _typeName;
        private IValue _value;
        private XSSimpleTypeDefinition _schemaType;

        private XSAttributeDeclaration()
        {
            _attribute = new XmlSchemaAttribute();
            _constraint = XSConstraint.Default;

            Components = new XSComponentFixedList();
        }

        internal XSAttributeDeclaration(XmlSchemaAttribute xmlAttribute)
            : this()
        {
            _attribute = xmlAttribute;

            if (_attribute.Annotation is XmlSchemaAnnotation annotation)
            {
                _annotation = XMLSchemaSerializer.CreateXSAnnotation(annotation);
                _annotation.BindToContainer(RootContainer, this);
            }

            if (_attribute.SchemaTypeName is XmlQualifiedName schemaTypeName)
                _typeName = XMLSchemaSerializer.CreateXMLExpandedName(schemaTypeName);

            if (_attribute.RefName is XmlQualifiedName refName)
                _refName = XMLSchemaSerializer.CreateXMLExpandedName(refName);

            if (_attribute.SchemaType is XmlSchemaSimpleType schemaType)
                _schemaType = XMLSchemaSerializer.CreateXSSimpleTypeDefinition(schemaType);
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
                XSAnnotation.SetComponentAnnotation(_annotation, _attribute);
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
        public XSComponentType ComponentType => XSComponentType.AttributeDeclaration;

        [ContextProperty("URIПространстваИмен", "NamespaceURI")]
        public string URIПространстваИмен => _attribute.SourceUri;

        [ContextProperty("Имя", "Name")]
        public string Name
        {
            get => _attribute.Name;
            set => _attribute.Name = value;
        }

        [ContextProperty("АнонимноеОпределениеТипа", "AnonymousTypeDefinition")]
        public XSSimpleTypeDefinition AnonymousTypeDefinition
        {
            get => _schemaType;
            set
            {
                _schemaType = value;
                if (_schemaType is XSSimpleTypeDefinition simpleType)
                {
                    simpleType.BindToContainer(RootContainer, this);
                    _attribute.SchemaType = simpleType.SchemaObject;
                }
                else
                    _attribute.SchemaType = null;
            }
        }

        [ContextProperty("Значение", "Value")]
        public IValue Value
        {
            get => _value;
            set
            {
                _value = value;
                if (_constraint == XSConstraint.Fixed)
                    _attribute.FixedValue = XMLSchema.XMLStringIValue(_value);
                else
                    _attribute.DefaultValue = XMLSchema.XMLStringIValue(_value);
            }
        }

        [ContextProperty("ИмяТипа", "TypeName")]
        public XMLExpandedName TypeName
        {
            get => _typeName;
            set
            {
                _typeName = value;
                _attribute.SchemaTypeName = _typeName.NativeValue;
            }
        }

        [ContextProperty("ЛексическоеЗначение", "LexicalValue")]
        public string LexicalValue
        {
            get => _constraint == XSConstraint.Fixed ? _attribute.FixedValue : _attribute.DefaultValue;
            set
            {
                if (_constraint == XSConstraint.Fixed)
                    _attribute.FixedValue = value;
                else
                    _attribute.DefaultValue = value;
            }
        }

        [ContextProperty("ОбластьВидимости", "Scope")]
        public XSAttributeDeclaration Scope { get; }

        [ContextProperty("Ограничение", "Constraint")]
        public XSConstraint Constraint
        {
            get => _constraint;
            set
            {
                _constraint = value;
                if (_constraint == XSConstraint.Default)
                    _attribute.FixedValue = null;

                else if (_constraint == XSConstraint.Fixed)
                    _attribute.DefaultValue = null;

                else
                {
                    _attribute.FixedValue = null;
                    _attribute.DefaultValue = null;
                }
            }
        }

        [ContextProperty("Ссылка", "Reference")]
        public XMLExpandedName Reference
        {
            get => _refName;
            set
            {
                _refName = value;
                _attribute.RefName = _refName.NativeValue;
            }
        }

        [ContextProperty("Форма", "Form")]
        public XSForm Form
        {
            get => XSForm.FromNativeValue(_attribute.Form);
            set => _attribute.Form = XSForm.ToNativeValue(value);
        }

        [ContextProperty("ЭтоГлобальноеОбъявление", "IsGlobal")]
        public bool IsGlobal => Container == Schema;

        [ContextProperty("ЭтоСсылка", "IsReference")]
        public bool IsReference => _refName != null;

        #endregion

        #region Methods

        [ContextMethod("КлонироватьКомпоненту", "CloneComponent")]
        public IXSComponent CloneComponent(bool recursive = true) => throw new NotImplementedException();

        [ContextMethod("ОбновитьЭлементDOM", "UpdateDOMElement")]
        public void UpdateDOMElement() => throw new NotImplementedException();

        [ContextMethod("Содержит", "Contains")]
        public bool Contains(IXSComponent component) => Components.Contains(component);

        [ContextMethod("ОпределениеТипа", "TypeDefinition")]
        public IXSType TypeDefinition() => throw new NotImplementedException();

        [ContextMethod("РазрешитьСсылку", "ResolveReference")]
        public XSAttributeDeclaration ResolveReference() => throw new NotImplementedException();

        #endregion

        #region Constructors

        [ScriptConstructor(Name = "По умолчанию")]
        public static XSAttributeDeclaration Constructor() => new XSAttributeDeclaration();

        #endregion

        #endregion

        #region IXSComponent

        public void BindToContainer(IXSComponent rootContainer, IXSComponent container)
        {
            RootContainer = rootContainer;
            Container = container;
        }

        XmlSchemaObject IXSComponent.SchemaObject => _attribute;
        public XmlSchemaAttribute SchemaObject => _attribute;

        #endregion
    }
}
