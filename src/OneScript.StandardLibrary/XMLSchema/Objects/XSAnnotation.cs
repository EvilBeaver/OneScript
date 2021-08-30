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
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.XMLSchema.Objects
{
    [ContextClass("АннотацияXS", "XSAnnotation")]
    public class XSAnnotation : AutoContext<XSAnnotation>, IXSComponent
    {
        internal readonly XmlSchemaAnnotation InternalObject;

        private XSAnnotation()
        {
            InternalObject = new XmlSchemaAnnotation();

            Content = new XSComponentList();
            Content.Cleared += Content_Cleared;
            Content.Inserted += Content_Inserted;

            Components = new XSComponentFixedList();
        }

        internal XSAnnotation(XmlSchemaAnnotation annotation)
            : this()
        {
            InternalObject = annotation;

            Content.Inserted -= Content_Inserted;
            foreach (XmlSchemaObject item in InternalObject.Items)
            {
                IXSComponent component = XMLSchemaSerializer.CreateInstance(item);
                component.BindToContainer(RootContainer, this);
                Content.Add(component);
                Components.Add(component);
            }
            Content.Inserted += Content_Inserted;
        }

        internal static void SetComponentAnnotation(XSAnnotation annotation, XmlSchemaAnnotated annotatedObject)
            => annotatedObject.Annotation = annotation?.SchemaObject;

        #region OneScript

        #region Properties

        [ContextProperty("Аннотация", "Annotation")]
        public XSAnnotation Annotation => null;

        [ContextProperty("Компоненты", "Components")]
        public XSComponentFixedList Components { get; }

        [ContextProperty("Контейнер", "Container")]
        public IXSComponent Container { get; private set; }

        [ContextProperty("КорневойКонтейнер", "RootContainer")]
        public IXSComponent RootContainer { get; private set; }

        [ContextProperty("Схема", "Schema")]
        public XMLSchema Schema => RootContainer.Schema;

        [ContextProperty("ТипКомпоненты", "ComponentType")]
        public XSComponentType ComponentType => XSComponentType.Annotation;

        [ContextProperty("Состав", "Content")]
        public XSComponentList Content { get; }

        #endregion

        #region Methods

        [ContextMethod("КлонироватьКомпоненту", "CloneComponent")]
        public IXSComponent CloneComponent(bool recursive = true) => throw new NotImplementedException();

        [ContextMethod("ОбновитьЭлементDOM", "UpdateDOMElement")]
        public void UpdateDOMElement() => throw new NotImplementedException();

        [ContextMethod("Содержит", "Contains")]
        public bool Contains(IXSComponent component) => Components.Contains(component);

        #region Constructors

        [ScriptConstructor(Name = "По умолчанию")]
        public static XSAnnotation Constructor() => new XSAnnotation();

        #endregion

        #endregion

        #endregion

        #region IXSComponent

        XmlSchemaObject IXSComponent.SchemaObject => InternalObject;
        public XmlSchemaAnnotation SchemaObject => InternalObject;

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

            if (!(component is IXSAnnotationItem))
                throw RuntimeException.InvalidArgumentType();

            component.BindToContainer(RootContainer, this);
            Components.Add(component);
            InternalObject.Items.Add(component.SchemaObject);
        }

        private void Content_Cleared(object sender, EventArgs e)
        {
            Components.Clear();
            InternalObject.Items.Clear();
        }

        #endregion
    }
}
