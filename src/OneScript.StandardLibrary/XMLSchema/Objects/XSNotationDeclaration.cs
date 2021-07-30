/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Xml.Schema;
using OneScript.Contexts;
using OneScript.StandardLibrary.XMLSchema.Collections;
using OneScript.StandardLibrary.XMLSchema.Enumerations;
using OneScript.StandardLibrary.XMLSchema.Interfaces;
using OneScript.Types;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.XMLSchema.Objects
{
    [ContextClass("ОбъявлениеНотацииXS", "XSNotationDeclaration")]
    public class XSNotationDeclaration : AutoContext<XSNotationDeclaration>, IXSAnnotated, IXSNamedComponent
    {
        private readonly XmlSchemaNotation _notation;
        private XSAnnotation _annotation;

        private XSNotationDeclaration() => _notation = new XmlSchemaNotation();

        internal XSNotationDeclaration(XmlSchemaNotation xmlNotation)
        {
            _notation = xmlNotation;

            if (_notation.Annotation is XmlSchemaAnnotation annotation)
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
                XSAnnotation.SetComponentAnnotation(_annotation, _notation);
            }
        }

        [ContextProperty("Компоненты", "Components")]
        public XSComponentFixedList Components => default(XSComponentFixedList);

        [ContextProperty("Контейнер", "Container")]
        public IXSComponent Container { get; private set; }

        [ContextProperty("КорневойКонтейнер", "RootContainer")]
        public IXSComponent RootContainer { get; private set; }

        [ContextProperty("Схема", "Schema")]
        public XMLSchema Schema => RootContainer.Schema;

        [ContextProperty("ТипКомпоненты", "ComponentType")]
        public XSComponentType ComponentType => XSComponentType.NotationDeclaration;

        [ContextProperty("Имя", "Name")]
        public string Name
        {
            get => _notation.Name;
            set => _notation.Name = value;
        }

        [ContextProperty("ПубличныйИдентификатор", "PublicId")]
        public string PublicId
        {
            get => _notation.Public;
            set => _notation.Public = value;
        }

        [ContextProperty("СистемныйИдентификатор", "SystemId")]
        public string SystemId
        {
            get => _notation.System;
            set => _notation.System = value;
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
        public static XSNotationDeclaration Constructor()
            => new XSNotationDeclaration();

        #endregion

        #endregion

        #region IXSComponent

        XmlSchemaObject IXSComponent.SchemaObject => _notation;

        void IXSComponent.BindToContainer(IXSComponent rootContainer, IXSComponent container)
        {
            RootContainer = rootContainer;
            Container = container;
        }

        #endregion
    }
}
