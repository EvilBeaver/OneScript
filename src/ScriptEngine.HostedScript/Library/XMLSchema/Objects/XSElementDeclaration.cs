using System;
using System.Xml.Schema;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("ОбъявлениеЭлементаXS", "XSElementDeclaration")]
    public class XSElementDeclaration : AutoContext<XSElementDeclaration>, IXSFragment
    {

        private readonly XmlSchemaElement _element;
        private XSAnnotation _annotation;
        private IXSComponent _container;
        private IXSComponent _rootContainer;
        
        private XSElementDeclaration() => _element = new XmlSchemaElement();

        #region OneScript

        #region Properties

        [ContextProperty("Аннотация", "Annotation")]
        public XSAnnotation Annotation
        {
            get => _annotation;
            set
            {
                _annotation = value;
                _element.Annotation = value.InternalObject;
            }
        }

        [ContextProperty("Компоненты", "Components")]
        public XSComponentFixedList Components { get; }

        [ContextProperty("Контейнер", "Container")]
        public IValue Container => _container;

        [ContextProperty("КорневойКонтейнер", "RootContainer")]
        public IValue RootContainer => _rootContainer;

        [ContextProperty("Схема", "Schema")]
        public XMLSchema Schema => _rootContainer.Schema;

        [ContextProperty("ТипКомпоненты", "ComponentType")]
        public XSComponentType ComponentType => XSComponentType.ElementDeclaration;

        [ContextProperty("URIПространстваИмен", "NamespaceURI")]
        public string URIПространстваИмен => _element.SourceUri;

        [ContextProperty("Имя", "Name")]
        public string Name
        {
            get => _element.Name;
            set => _element.Name = value;
        }


        //АнонимноеОпределениеТипа(AnonymousTypeDefinition)
        //Значение(Value)
        //ИмяТипа(TypeName)
        //ЛексическоеЗначение(LexicalValue)
        //ОбластьВидимости(Scope)
        //Ограничение(Constraint)
        //Ссылка(Reference)
        //Форма(Form)
        //ЭтоГлобальноеОбъявление(IsGlobal)
        //ЭтоСсылка(IsReference)

        [ContextProperty("Абстрактный", "Abstract")]
        public bool Abstract
        {
            get => _element.IsAbstract;
            set => _element.IsAbstract = value;
        }

        //Блокировка(Block)

        [ContextProperty("ВозможноПустой", "Nillable")]
        public bool Nillable
        {
            get => _element.IsNillable;
            set => _element.IsNillable = value;
        }

        //Завершенность(Final)
        //ИсключенияГруппПодстановки(SubstitutionGroupExclusions)
        //НедопустимыеПодстановки(DisallowedSubstitutions)
        //ОграниченияИдентичности(IdentityConstraints)
        //ПрисоединениеКГруппеПодстановки(SubstitutionGroupAffiliation)

        #endregion

        #region Methods

        [ContextMethod("КлонироватьКомпоненту", "CloneComponent")]
        public IValue CloneComponent(IValue recursive = null)
        {
            throw new NotImplementedException();
        }

        [ContextMethod("ОбновитьЭлементDOM", "UpdateDOMElement")]
        public void UpdateDOMElement()
        {
            throw new NotImplementedException();
        }

        [ContextMethod("Содержит", "Contains")]
        public bool Contains(IValue component) => (component == this);

        //ОпределениеТипа(TypeDefinition)
        //РазрешитьСсылку(ResolveReference)
        //ЭтоОбъявлениеЗациклено(IsCircular)

        #endregion

        #region Constructors

        [ScriptConstructor(Name = "По умолчанию")]
        public static XSElementDeclaration Constructor() => new XSElementDeclaration();

        #endregion

        #endregion

        #region IXSComponent

        XmlSchemaObject IXSComponent.SchemaObject => _element;

        void IXSComponent.BindToContainer(IXSComponent rootContainer, IXSComponent container)
        {
            _rootContainer = rootContainer;
            _container = container;
        }

        #endregion

    }
}
