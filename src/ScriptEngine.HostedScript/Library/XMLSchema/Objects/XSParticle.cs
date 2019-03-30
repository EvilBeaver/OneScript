using System;
using System.Xml.Schema;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("ФрагментXS", "XSParticle")]
    public class XSParticle : AutoContext<XSParticle>, IXSComponent
    {
        private IXSFragment _term;
        private decimal _minOccurs;
        private decimal _maxOccurs;

        private XSParticle()
        {
            _minOccurs = 1;
            _maxOccurs = 1;

            Components = new XSComponentFixedList();
        }

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
        public XSComponentType ComponentType => XSComponentType.Particle;

        [ContextProperty("ЭлементDOM", "DOMElement")]
        public IValue DOMElement => ValueFactory.Create();

        [ContextProperty("МаксимальноВходит", "MaxOccurs")]
        public decimal MaxOccurs
        {
            get => _maxOccurs;
            set
            {
                _maxOccurs = value;
                if (_term?.SchemaObject is XmlSchemaParticle _particle)
                    _particle.MaxOccurs = _maxOccurs;
            }
        }

        [ContextProperty("МинимальноВходит", "MinOccurs")]
        public decimal MinOccurs
        {
            get => _minOccurs;
            set
            {
                _minOccurs = value;
                if (_term?.SchemaObject is XmlSchemaParticle _particle)
                    _particle.MinOccurs = _minOccurs;
            }
        }

        [ContextProperty("Часть", "Term")]
        public IXSFragment Term
        {
            get => _term;
            set
            {
                _term = value;
                Components.Clear();
                if(_term != null)
                {
                    _term.BindToContainer(RootContainer, this);
                    Components.Add(_term);
                    if (_term.SchemaObject is XmlSchemaParticle _particle)
                    {
                        _particle.MinOccurs = _minOccurs;
                        _particle.MaxOccurs = _maxOccurs;
                    }
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
        public bool Contains(IXSComponent component) => Components.Contains(component);

        [ContextMethod("ДопускаетсяПустой", "IsEmptiable")]
        public bool IsEmptiable() => throw new NotImplementedException();

        [ContextMethod("ЯвляетсяПодмножеством", "IsSubset")]
        public bool IsSubset() => throw new NotImplementedException();

        #endregion

        #region Constructors

        [ScriptConstructor(Name = "По умолчанию")]
        public static XSParticle Constructor() => new XSParticle();

        #endregion

        #endregion

        #region IXSComponent

        XmlSchemaObject IXSComponent.SchemaObject => _term?.SchemaObject;

        void IXSComponent.BindToContainer(IXSComponent rootContainer, IXSComponent container)
        {
            RootContainer = rootContainer;
            Container = container;
        }

        #endregion
    }
}
