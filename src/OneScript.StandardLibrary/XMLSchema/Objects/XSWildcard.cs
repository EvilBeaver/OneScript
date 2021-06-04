/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Xml.Schema;
using OneScript.Commons;
using OneScript.StandardLibrary.Collections;
using OneScript.StandardLibrary.XMLSchema.Collections;
using OneScript.StandardLibrary.XMLSchema.Enumerations;
using OneScript.StandardLibrary.XMLSchema.Interfaces;
using OneScript.Types;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.XMLSchema.Objects
{
    [ContextClass("МаскаXS", "XSWildcard")]
    public class XSWildcard : AutoContext<XSWildcard>, IXSAnnotated, IXSFragment
    {
        private XmlSchemaAnnotated _wildcard;
        private XSAnnotation _annotation;
        private XSProcessContents _processContents;
        private string _namespace;
        private XSNamespaceConstraintCategory _namespaceConstraint;

        private XSWildcard() { }

        internal XSWildcard(XmlSchemaAny xmlAny)
        {
            _wildcard = xmlAny;

            if (_wildcard.Annotation is XmlSchemaAnnotation annotation)
            {
                _annotation = XMLSchemaSerializer.CreateXSAnnotation(annotation);
                _annotation.BindToContainer(RootContainer, this);
            }

            _processContents = (XSProcessContents)xmlAny.ProcessContents;
            _namespace = xmlAny.Namespace;

            SetNamespaceConstraint();
        }
        internal XSWildcard(XmlSchemaAnyAttribute xmlAnyAttribute)
        {
            _wildcard = xmlAnyAttribute;

            if (_wildcard.Annotation is XmlSchemaAnnotation annotation)
            {
                _annotation = XMLSchemaSerializer.CreateXSAnnotation(annotation);
                _annotation.BindToContainer(RootContainer, this);
            }

            _processContents = (XSProcessContents)xmlAnyAttribute.ProcessContents;
            _namespace = xmlAnyAttribute.Namespace;

            SetNamespaceConstraint();
        }

        private void OnSetProcessContents()
        {
            XmlSchemaContentProcessing processContents = (XmlSchemaContentProcessing)_processContents;

            if (_wildcard is XmlSchemaAny xmlAny)
                xmlAny.ProcessContents = processContents;

            else if (_wildcard is XmlSchemaAnyAttribute xmlAnyAttribute)
                xmlAnyAttribute.ProcessContents = processContents;
        }

        private void OnSetNamespace()
        {
            SetNamespaceConstraint();

            if (_wildcard is XmlSchemaAny xmlAny)
                xmlAny.Namespace = _namespace;

            else if (_wildcard is XmlSchemaAnyAttribute xmlAnyAttribute)
                xmlAnyAttribute.Namespace = _namespace;
        }

        private void SetNamespaceConstraint()
        {
            switch (_namespace)
            {
                case "##targetNamespace":
                    _namespaceConstraint = XSNamespaceConstraintCategory.Set;
                    break;

                case "##any":
                    _namespaceConstraint = XSNamespaceConstraintCategory.Any;
                    break;

                case "##other":
                    _namespaceConstraint = XSNamespaceConstraintCategory.Not;
                    break;

                default:
                    _namespaceConstraint = XSNamespaceConstraintCategory.EmptyRef;
                    break;
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
                XSAnnotation.SetComponentAnnotation(_annotation, _wildcard);
            }
        }

        [ContextProperty("Компоненты", "Components")]
        public XSComponentFixedList Components => null;

        [ContextProperty("Контейнер", "Container")]
        public IXSComponent Container { get; private set; }

        [ContextProperty("КорневойКонтейнер", "RootContainer")]
        public IXSComponent RootContainer { get; private set; }

        [ContextProperty("Схема", "Schema")]
        public XMLSchema Schema => RootContainer.Schema;

        [ContextProperty("ТипКомпоненты", "ComponentType")]
        public XSComponentType ComponentType => XSComponentType.Wildcard;

        [ContextProperty("ВидОбработкиСодержимого", "ProcessContents")]
        public XSProcessContents ProcessContents
        {
            get => _processContents;
            set
            {
                _processContents = value;
                OnSetProcessContents();
            }
        }

        [ContextProperty("КатегорияОграниченияПространствИмен", "NamespaceConstraintCategory")]
        public XSNamespaceConstraintCategory NamespaceConstraintCategory => _namespaceConstraint;

        [ContextProperty("ЛексическоеЗначениеОграниченияПространствИмен", "LexicalNamespaceConstraint")]
        public string LexicalNamespaceConstraint
        {
            get => _namespace;
            set
            {
                _namespace = value;
                OnSetNamespace();
            }
        }

        #endregion

        #region Methods

        [ContextMethod("КлонироватьКомпоненту", "CloneComponent")]
        public IXSComponent CloneComponent(bool recursive = true) => throw new NotImplementedException();

        [ContextMethod("ОбновитьЭлементDOM", "UpdateDOMElement")]
        public void UpdateDOMElement() => throw new NotImplementedException();

        [ContextMethod("Содержит", "Contains")]
        public bool Contains(IXSComponent component) => false;

        [ContextMethod("Допускает", "Allow")]
        public bool Allow(string namespaseUri) => throw new NotImplementedException();

        [ContextMethod("ОграничениеПространствИмен", "NamespaceConstraint")]
        public ArrayImpl NamespaceConstraint() => throw new NotImplementedException();

        [ContextMethod("ЭтаМаскаПодмножество", "IsWildcardSubset")]
        public bool IsWildcardSubset(XSWildcard wildcard) => throw new NotImplementedException();

        #endregion

        #region Constructors

        [ScriptConstructor(Name = "По умолчанию")]
        public static XSWildcard Constructor() => new XSWildcard();

        #endregion

        #endregion

        #region IXSComponent

        public void BindToContainer(IXSComponent rootContainer, IXSComponent container)
        {
            RootContainer = rootContainer;
            Container = container;

            if (container is XSParticle)
            {
                if (!(_wildcard is XmlSchemaAny))
                {
                    _wildcard = new XmlSchemaAny()
                    {
                        Annotation = _annotation?.InternalObject
                    };
                    OnSetProcessContents();
                    OnSetNamespace();
                }
            }
            else if ((container is XSComplexTypeDefinition) || (container is XSAttributeGroupDefinition))
            {
                if (!(_wildcard is XmlSchemaAnyAttribute))
                {
                    _wildcard = new XmlSchemaAnyAttribute()
                    {
                        Annotation = _annotation?.InternalObject
                    };
                    OnSetProcessContents();
                    OnSetNamespace();
                }
            }
            else
            {
                throw RuntimeException.InvalidArgumentType();
            }
        }

        public XmlSchemaObject SchemaObject => _wildcard;

        #endregion
    }
}