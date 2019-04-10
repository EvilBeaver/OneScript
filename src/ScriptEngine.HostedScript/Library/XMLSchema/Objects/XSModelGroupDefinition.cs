/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Xml.Schema;
using ScriptEngine.HostedScript.Library.Xml;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("ОпределениеГруппыМоделиXS", "XSModelGroupDefinition")]
    public class XSModelGroupDefinition : AutoContext<XSModelGroupDefinition>, IXSComponent, IXSNamedComponent
    {
        private XmlSchemaAnnotated _group;
        private string _name;
        private XSParticle _modelGroup;
        private XMLExpandedName _reference;

        private XSModelGroupDefinition()
        {
            _group = new XmlSchemaGroup();
        }

        #region OneScript

        #region Properties

        [ContextProperty("Аннотация", "Annotation")]
        public XSAnnotation Annotation => null;

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
        public XSParticle ModelGroup
        {
            get => _modelGroup;
            set
            {
                _modelGroup = value;
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
