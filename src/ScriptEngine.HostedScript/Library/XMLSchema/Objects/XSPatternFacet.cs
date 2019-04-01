using System;
using System.Xml.Schema;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("ФасетОбразцаXS", "XSPatternFacet")]
    public class XSPatternFacet : AutoContext<XSPatternFacet>, IXSFacet
    {
        private readonly XmlSchemaPatternFacet _facet;
        private XSAnnotation _annotation;

        private XSPatternFacet() => _facet = new XmlSchemaPatternFacet();

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
        public XSComponentType ComponentType => XSComponentType.PatternFacet;

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
        public string Value
        {
            get => _facet.Value;
            set => _facet.Value = value;
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
        public static XSPatternFacet Constructor() => new XSPatternFacet();

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
