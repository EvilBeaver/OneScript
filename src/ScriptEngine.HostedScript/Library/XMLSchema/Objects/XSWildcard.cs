/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Xml.Schema;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("МаскаXS", "XSWildcard")]
    public class XSWildcard : AutoContext<XSWildcard>, IXSAnnotated, IXSFragment
    {
        private XmlSchemaAnnotated _wildcard;
        private XSAnnotation _annotation;

        private XSWildcard() { }

        #region OneScript

        #region Properties

        [ContextProperty("Аннотация", "Annotation")]
        public XSAnnotation Annotation
        {
            get => _annotation;
            set
            {
                _annotation = value;
                _wildcard.Annotation = value.InternalObject;
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
        public XSProcessContents ProcessContents { get; set; }

        [ContextProperty("КатегорияОграниченияПространствИмен", "NamespaceConstraintCategory")]
        public XSNamespaceConstraintCategory NamespaceConstraintCategory
        {
            get
            {
                switch (LexicalNamespaceConstraint)
                {
                    case "##targetNamespace":
                        return XSNamespaceConstraintCategory.Set;

                    case "##any":
                        return XSNamespaceConstraintCategory.Any;

                    case "##other":
                        return XSNamespaceConstraintCategory.Not;

                    default:
                        return XSNamespaceConstraintCategory.EmptyRef;
                }
            }
        }

        [ContextProperty("ЛексическоеЗначениеОграниченияПространствИмен", "LexicalNamespaceConstraint")]
        public string LexicalNamespaceConstraint { get; set; }

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
                _wildcard = new XmlSchemaAny()
                {
                    Annotation = Annotation?.InternalObject,
                    ProcessContents = (XmlSchemaContentProcessing)ProcessContents,
                    Namespace = LexicalNamespaceConstraint
                };
            }
            else if ((container is XSComplexTypeDefinition) || (container is XSAttributeGroupDefinition))
            {
                _wildcard = new XmlSchemaAnyAttribute()
                {
                    Annotation = Annotation?.InternalObject,
                    ProcessContents = (XmlSchemaContentProcessing)ProcessContents,
                    Namespace = LexicalNamespaceConstraint
                };
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