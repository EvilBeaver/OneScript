/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Xml;
using System.Xml.Schema;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("ФасетОбщегоКоличестваРазрядовXS", "XSTotalDigitsFacet")]
    public class XSTotalDigitsFacet : AutoContext<XSTotalDigitsFacet>, IXSFacet
    {
        private readonly XmlSchemaTotalDigitsFacet _facet;
        private XSAnnotation _annotation;

        private XSTotalDigitsFacet() => _facet = new XmlSchemaTotalDigitsFacet();

        #region OneScript

        #region Properties

        [ContextProperty("Аннотация", "Annotation")]
        public XSAnnotation Annotation
        {
            get => _annotation;
            set
            {
                _annotation = value;
                _facet.Annotation = value.InternalObject;
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
        public XSComponentType ComponentType => XSComponentType.TotalDigitsFacet;

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
        public decimal Value
        {
            get => XmlConvert.ToDecimal(_facet.Value);
            set => _facet.Value = XmlConvert.ToString(value);
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
        public static XSTotalDigitsFacet Constructor() => new XSTotalDigitsFacet();

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
