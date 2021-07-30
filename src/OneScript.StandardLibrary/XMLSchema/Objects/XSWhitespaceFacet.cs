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
using OneScript.Types;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.XMLSchema.Objects
{
    [ContextClass("ФасетПробельныхСимволовXS", "XSWhitespaceFacet")]
    public class XSWhitespaceFacet : AutoContext<XSWhitespaceFacet>, IXSFacet
    {
        private readonly XmlSchemaWhiteSpaceFacet _facet;
        private XSAnnotation _annotation;

        private XSWhitespaceFacet() => _facet = new XmlSchemaWhiteSpaceFacet();

        internal XSWhitespaceFacet(XmlSchemaWhiteSpaceFacet whitespaceFacet)
        {
            _facet = whitespaceFacet;

            if (_facet.Annotation is XmlSchemaAnnotation annotation)
            {
                _annotation = XMLSchemaSerializer.CreateXSAnnotation(annotation);
                _annotation.BindToContainer(RootContainer, this);
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
                XSAnnotation.SetComponentAnnotation(_annotation, _facet);
            }
        }

        [ContextProperty("Компоненты", "Components")]
        public XSComponentFixedList Components => null;

        [ContextProperty("Контейнер", "Container")]
        public IXSComponent Container => SimpleTypeDefinition;

        [ContextProperty("КорневойКонтейнер", "RootContainer")]
        public IXSComponent RootContainer { get; private set; }

        [ContextProperty("Схема", "Schema")]
        public XMLSchema Schema => RootContainer.Schema;

        [ContextProperty("ТипКомпоненты", "ComponentType")]
        public XSComponentType ComponentType => XSComponentType.WhitespaceFacet;

        [ContextProperty("ЛексическоеЗначение", "LexicalValue")]
        public string LexicalValue
        {
            get => _facet.Value;
            set => _facet.Value = value;
        }

        [ContextProperty("ОпределениеПростогоТипа", "SimpleTypeDefinition")]
        public XSSimpleTypeDefinition SimpleTypeDefinition { get; private set; }

        [ContextProperty("Фиксированный", "Fixed")]
        public bool Fixed
        {
            get => _facet.IsFixed;
            set => _facet.IsFixed = value;
        }

        [ContextProperty("Значение", "Value")]
        public XSWhitespaceHandling Value
        {
            get
            {
                switch (_facet.Value)
                {
                    case "collapse":
                        return XSWhitespaceHandling.Collapse;

                    case "preserve":
                        return XSWhitespaceHandling.Preserve;

                    case "replace":
                        return XSWhitespaceHandling.Replace;

                    default:
                        return XSWhitespaceHandling.Collapse;
                }
            }
            set
            {
                switch (value)
                {
                    case XSWhitespaceHandling.Collapse:
                        _facet.Value = "collapse";
                        break;

                    case XSWhitespaceHandling.Preserve:
                        _facet.Value = "preserve";
                        break;

                    case XSWhitespaceHandling.Replace:
                        _facet.Value = "replace";
                        break;

                    default:
                        _facet.Value = null;
                        break;
                }
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

        #endregion

        #region Constructors

        [ScriptConstructor(Name = "По умолчанию")]
        public static XSWhitespaceFacet Constructor() => new XSWhitespaceFacet();

        #endregion

        #endregion

        #region IXSComponent

        XmlSchemaObject IXSComponent.SchemaObject => _facet;

        void IXSComponent.BindToContainer(IXSComponent rootContainer, IXSComponent container)
        {
            if (!(container is XSSimpleTypeDefinition))
                throw RuntimeException.InvalidArgumentType();

            RootContainer = rootContainer;
            SimpleTypeDefinition = container as XSSimpleTypeDefinition;
        }

        #endregion
    }
}
