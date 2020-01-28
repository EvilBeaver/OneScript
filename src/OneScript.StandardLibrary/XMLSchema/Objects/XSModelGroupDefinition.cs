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
    [ContextClass("ОпределениеГруппыМоделиXS", "XSModelGroupDefinition")]
    public class XSModelGroupDefinition : AutoContext<XSModelGroupDefinition>, IXSFragment, IXSNamedComponent
    {
        private XmlSchemaAnnotated _group;
        private string _name;
        private IXSComponent _modelGroup;
        private XMLExpandedName _reference;
        private XSAnnotation _annotation;

        private XSModelGroupDefinition() => _group = new XmlSchemaGroup();

        internal XSModelGroupDefinition(XmlSchemaGroup xmlGroup)
        {
            _group = xmlGroup;
            _name = xmlGroup.Name;

            if (_group.Annotation is XmlSchemaAnnotation annotation)
            {
                _annotation = XMLSchemaSerializer.CreateXSAnnotation(annotation);
                _annotation.BindToContainer(RootContainer, this);
            }

            if (xmlGroup.Particle is XmlSchemaGroupBase xmlGroupBase)
            {
                IXSComponent component = XMLSchemaSerializer.CreateInstance(xmlGroupBase);

                if (component is XSParticle particle)
                    _modelGroup = particle;

                else if (component is XSModelGroup modelGroup)
                    _modelGroup = modelGroup;
            }
        }

        internal XSModelGroupDefinition(XmlSchemaGroupRef xmlGroupRef)
        {
            _group = xmlGroupRef;

            if (_group.Annotation is XmlSchemaAnnotation annotation)
            {
                _annotation = XMLSchemaSerializer.CreateXSAnnotation(annotation);
                _annotation.BindToContainer(RootContainer, this);
            }

            if (xmlGroupRef.RefName is XmlQualifiedName qualifiedName)
                _reference = XMLSchemaSerializer.CreateXMLExpandedName(qualifiedName);
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
                XSAnnotation.SetComponentAnnotation(_annotation, _group);
            }
        }

        [ContextProperty("Компоненты", "Components")]
        public XSComponentFixedList Components => null;

        [ContextProperty("Контейнер", "Container")]
        public IXSComponent Container { get; private set; }

        [ContextProperty("КорневойКонтейнер", "RootContainer")]
        public IXSComponent RootContainer { get; private set; }

        [ContextProperty("Схема", "Schema")]
        public XMLSchema Schema => RootContainer?.Schema;

        [ContextProperty("ТипКомпоненты", "ComponentType")]
        public XSComponentType ComponentType => XSComponentType.ModelGroupDefinition;

        [ContextProperty("URIПространстваИмен", "NamespaceURI")]
        public string NamespaceURI { get; }

        [ContextProperty("Имя", "Name")]
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                if (_group is XmlSchemaGroup group)
                    group.Name = _name;
            }
        }

        [ContextProperty("ГруппаМодели", "ModelGroup")]
        public IXSComponent ModelGroup
        {
            get => _modelGroup;
            set
            {
                if (value is XSParticle particle)
                    _modelGroup = particle;

                else if (value is XSModelGroup modelGroup)
                    _modelGroup = modelGroup;

                else if (value == null)
                    _modelGroup = null;

                else
                    throw RuntimeException.InvalidArgumentType();

                if (_group is XmlSchemaGroup group)
                    group.Particle = _modelGroup?.SchemaObject as XmlSchemaGroupBase;
            }
        }

        [ContextProperty("Ссылка", "Reference")]
        public XMLExpandedName Reference
        {
            get => _reference;
            set
            {
                _reference = value;
                if (_group is XmlSchemaGroupRef groupRef)
                    groupRef.RefName = _reference.NativeValue;
                else
                {
                    _group = new XmlSchemaGroupRef()
                    {
                        RefName = _reference?.NativeValue
                    };
                }
            }
        }

        [ContextProperty("ЭтоСсылка", "IsReference")]
        public bool IsReference => _group is XmlSchemaGroupRef;

        #endregion

        #region Methods

        [ContextMethod("КлонироватьКомпоненту", "CloneComponent")]
        public IXSComponent CloneComponent(bool recursive = true) => throw new NotImplementedException();

        [ContextMethod("ОбновитьЭлементDOM", "UpdateDOMElement")]
        public void UpdateDOMElement() => throw new NotImplementedException();

        [ContextMethod("Содержит", "Contains")]
        public bool Contains(IXSComponent component) => false;

        [ContextMethod("ЭтоОпределениеЗациклено", "IsCircular")]
        public bool IsCircular() => throw new NotImplementedException();

        [ContextMethod("РазрешитьСсылку", "ResolveReference")]

        public XSModelGroupDefinition ResolveReference() => throw new NotImplementedException();

        #endregion

        #region Constructors

        [ScriptConstructor(Name = "По умолчанию")]
        public static XSModelGroupDefinition Constructor() => new XSModelGroupDefinition();

        #endregion

        #endregion

        #region IXSComponent

        public void BindToContainer(IXSComponent rootContainer, IXSComponent container)
        {
            RootContainer = rootContainer;
            Container = container;
        }

        public XmlSchemaObject SchemaObject => _group;

        #endregion
    }
}
