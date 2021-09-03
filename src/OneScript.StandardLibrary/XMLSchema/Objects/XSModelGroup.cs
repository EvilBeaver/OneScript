/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Xml.Schema;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.StandardLibrary.XMLSchema.Collections;
using OneScript.StandardLibrary.XMLSchema.Enumerations;
using OneScript.StandardLibrary.XMLSchema.Interfaces;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.XMLSchema.Objects
{
    [ContextClass("ГруппаМоделиXS", "XSModelGroup")]
    public class XSModelGroup : AutoContext<XSModelGroup>, IXSFragment
    {
        private XmlSchemaGroupBase _group;
        private XSAnnotation _annotation;
        private XSCompositor _compositor;

        private XSModelGroup()
        {
            _compositor = XSCompositor.Sequence;
            _group = new XmlSchemaSequence();

            Components = new XSComponentFixedList();
            Particles = new XSComponentList();
            Particles.Inserted += Particles_Inserted;
            Particles.Cleared += Particles_Cleared;
        }

        internal XSModelGroup(XmlSchemaGroupBase groupBase)
            : this()
        {
            _group = groupBase;

            if (_group.Annotation is XmlSchemaAnnotation annotation)
            {
                _annotation = XMLSchemaSerializer.CreateXSAnnotation(annotation);
                _annotation.BindToContainer(RootContainer, this);
            }

            if (groupBase is XmlSchemaAll)
                _compositor = XSCompositor.All;

            else if (groupBase is XmlSchemaChoice)
                _compositor = XSCompositor.Choice;

            else if (groupBase is XmlSchemaSequence)
                _compositor = XSCompositor.Sequence;

            Particles.Inserted -= Particles_Inserted;

            foreach (XmlSchemaObject item in _group.Items)
            {
                IXSComponent component = XMLSchemaSerializer.CreateInstance(item);
                component.BindToContainer(RootContainer, this);
                Particles.Add(component);
                Components.Add(component);
            }

            Particles.Inserted += Particles_Inserted;
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
        public XSComponentFixedList Components { get; }

        [ContextProperty("Контейнер", "Container")]
        public IXSComponent Container { get; private set; }

        [ContextProperty("КорневойКонтейнер", "RootContainer")]
        public IXSComponent RootContainer { get; private set; }

        [ContextProperty("Схема", "Schema")]
        public XMLSchema Schema => RootContainer.Schema;

        [ContextProperty("ТипКомпоненты", "ComponentType")]
        public XSComponentType ComponentType => XSComponentType.ModelGroup;

        [ContextProperty("ЭлементDOM", "DOMElement")]
        public IValue DOMElement => ValueFactory.Create();

        [ContextProperty("ВидГруппы", "Compositor")]
        public XSCompositor Compositor
        {
            get => _compositor;
            set
            {
                _compositor = value;
                switch (_compositor)
                {
                    case XSCompositor.All:
                        _group = new XmlSchemaAll();
                        break;

                    case XSCompositor.Choice:
                        _group = new XmlSchemaChoice();
                        break;

                    case XSCompositor.Sequence:
                        _group = new XmlSchemaSequence();
                        break;

                    default:
                        throw RuntimeException.InvalidArgumentValue();
                }
            }
        }

        [ContextProperty("Фрагменты", "Particles")]
        public XSComponentList Particles { get; }

        #endregion

        #region Methods

        [ContextMethod("КлонироватьКомпоненту", "CloneComponent")]
        public IXSComponent CloneComponent(bool recursive = true) => throw new NotImplementedException();

        [ContextMethod("ОбновитьЭлементDOM", "UpdateDOMElement")]
        public void UpdateDOMElement() => throw new NotImplementedException();

        [ContextMethod("Содержит", "Contains")]
        public bool Contains(IXSComponent component) => Components.Contains(component);

        #endregion

        #region Constructors

        [ScriptConstructor(Name = "По умолчанию")]
        public static XSModelGroup Constructor() => new XSModelGroup();

        #endregion

        #endregion

        #region IXSComponent

        XmlSchemaObject IXSComponent.SchemaObject => _group;
        public XmlSchemaGroupBase SchemaObject => _group;

        void IXSComponent.BindToContainer(IXSComponent rootContainer, IXSComponent container)
        {
            RootContainer = rootContainer;
            Container = container;
        }

        #endregion

        #region XSComponentListEvents

        private void Particles_Inserted(object sender, XSComponentListEventArgs e)
        {
            IXSComponent component = e.Component;

            if (!(component is IXSFragment) && !(component is XSParticle))
                throw RuntimeException.InvalidArgumentType();

            component.BindToContainer(RootContainer, this);
            Components.Add(component);
            _group.Items.Add(component.SchemaObject);
        }

        private void Particles_Cleared(object sender, EventArgs e)
        {
            Components.Clear();
            _group.Items.Clear();
        }

        #endregion
    }
}
