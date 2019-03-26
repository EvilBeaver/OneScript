using System;
using System.Xml.Schema;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("ОбъявлениеЭлементаXS", "XSElementDeclaration")]
    public class XSElementDeclaration : AutoContext<XSElementDeclaration>, IXSFragment, IXSNamedComponent
    {

        private readonly XmlSchemaElement _element;
        private XSAnnotation _annotation;

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
        public IXSComponent Container { get; private set; }

        [ContextProperty("КорневойКонтейнер", "RootContainer")]
        public IXSComponent RootContainer { get; private set; }

        [ContextProperty("Схема", "Schema")]
        public XMLSchema Schema => RootContainer.Schema;

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
        public IXSComponent CloneComponent(bool recursive = true) => throw new NotImplementedException();

        [ContextMethod("ОбновитьЭлементDOM", "UpdateDOMElement")]
        public void UpdateDOMElement() => throw new NotImplementedException();

        [ContextMethod("Содержит", "Contains")]
        public bool Contains(IXSComponent component) => throw new NotImplementedException();

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
            RootContainer = rootContainer;
            Container = container;
        }

        #endregion
    }
}
