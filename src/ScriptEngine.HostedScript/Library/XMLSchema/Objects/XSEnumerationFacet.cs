using System;
using System.Xml;
using System.Xml.Schema;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("ФасетПеречисленияXS", "XSEnumerationFacet")]
    public class XSEnumerationFacet : AutoContext<XSEnumerationFacet>, IXSFacet
    {
        private readonly XmlSchemaEnumerationFacet _facet;
        private XSAnnotation _annotation;
        private IValue _value;

        private XSEnumerationFacet() => _facet = new XmlSchemaEnumerationFacet();

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
        public XSComponentType ComponentType => XSComponentType.EnumerationFacet;

        [ContextProperty("ЛексическоеЗначение", "LexicalValue")]
        public string LexicalValue
        {
            get => _facet.Value;
            set
            {
                _facet.Value = value;
                _value = ValueFactory.Create(value);
            }
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
        public IValue Value
        {
            get => _value;
            set
            {
                switch (value.DataType)
                {
                    case DataType.String:
                        _facet.Value = value.AsString();
                        break;

                    case DataType.Boolean:
                        _facet.Value = XmlConvert.ToString(value.AsBoolean());
                        break;

                    case DataType.Date:
                        _facet.Value = XmlConvert.ToString(value.AsDate(), XmlDateTimeSerializationMode.Unspecified);
                        break;

                    case DataType.Number:
                        _facet.Value = XmlConvert.ToString(value.AsNumber());
                        break;

                    default:
                        throw RuntimeException.InvalidArgumentType();
                }
                _value = value;
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
        public static XSEnumerationFacet Constructor() => new XSEnumerationFacet();

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
