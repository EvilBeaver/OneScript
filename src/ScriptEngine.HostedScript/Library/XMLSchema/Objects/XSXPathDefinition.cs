/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Xml.Schema;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("ФасетПробельныхСимволовXS", "XSWhitespaceFacet")]
    public class XSXPathDefinition : AutoContext<XSXPathDefinition>, IXSAnnotated, IXSNamedComponent
    {
        private readonly XmlSchemaXPath _xpath;
        private XSAnnotation _annotation;

        private XSXPathDefinition() => _xpath = new XmlSchemaXPath();
        
        #region OneScript

        #region Properties

        [ContextProperty("Аннотация", "Annotation")]
        public XSAnnotation Annotation
        {
            get => _annotation;
            set
            {
                _annotation = value;
                _xpath.Annotation = value.InternalObject;
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
        public XSComponentType ComponentType => XSComponentType.XPathDefinition;

        [ContextProperty("URIПространстваИмен", "NamespaceURI")]
        public string NamespaceURI => _xpath.SourceUri;

        [ContextProperty("Имя", "Name")]
        public string Name { get; set; }

        [ContextProperty("XPath")]
        public string XPath
        {
            get => _xpath.XPath;
            set => _xpath.XPath = value;
        }

        [ContextProperty("Вариант", "Variety")]
        public XSXPathVariety Variety { get; set; }

        #endregion

        #region Methods

        [ContextMethod("КлонироватьКомпоненту", "CloneComponent")]
        public IXSComponent CloneComponent(bool recursive = true) => throw new NotImplementedException();

        [ContextMethod("ОбновитьЭлементDOM", "UpdateDOMElement")]
        public void UpdateDOMElement() => throw new NotImplementedException();

        [ContextMethod("Содержит", "Contains")]
        public bool Contains(IXSComponent component) => false;

        #endregion

        #region Constructors

        [ScriptConstructor(Name = "По умолчанию")]
        public static XSXPathDefinition Constructor() => new XSXPathDefinition();

        #endregion

        #endregion

        #region IXSComponent

        void IXSComponent.BindToContainer(IXSComponent rootContainer, IXSComponent container)
        {
            RootContainer = rootContainer;
            Container = container;
        }

        XmlSchemaObject IXSComponent.SchemaObject => _xpath;

        #endregion
    }
}