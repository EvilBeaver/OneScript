/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Xml.Schema;
using OneScript.StandardLibrary.XMLSchema.Collections;
using OneScript.StandardLibrary.XMLSchema.Enumerations;
using OneScript.StandardLibrary.XMLSchema.Interfaces;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.XMLSchema.Objects
{
    [ContextClass("ИспользованиеАтрибутаXS", "XSAttributeUse")]
    public class XSAttributeUse : AutoContext<XSAttributeUse>, IXSComponent
    {

        private IValue _value;
        private string _lexicalValue;
        private XSAttributeDeclaration _attributeDeclaration;

        private XSAttributeUse()
        {
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
        public XSComponentType ComponentType => XSComponentType.AttributeUse;

        [ContextProperty("Значение", "Value")]
        public IValue Value
        {
            get => _value;
            set
            {
                _value = value;
                _lexicalValue = XMLSchema.XMLStringIValue(_value);
            }
        }

        [ContextProperty("Использование", "Use")]
        public XSAttributeUseCategory Use { get; set; }

        [ContextProperty("ЛексическоеЗначение", "LexicalValue")]
        public string LexicalValue
        {
            get => _lexicalValue;
            set
            {
                _lexicalValue = value;
                _value = ValueFactory.Create(_lexicalValue);
            }
        }

        [ContextProperty("ОбъявлениеАтрибута", "AttributeDeclaration")]
        public XSAttributeDeclaration AttributeDeclaration
        {
            get => _attributeDeclaration;
            set
            {
                _attributeDeclaration = value;
                if (_attributeDeclaration is XSAttributeDeclaration)
                {
                    _attributeDeclaration.BindToContainer(RootContainer, this);
                    var attribute = _attributeDeclaration.SchemaObject;
                    attribute.Use = (XmlSchemaUse)Use;
                }
            }
        }

        [ContextProperty("Обязательный", "IsRequired")]
        public bool IsRequired => Use == XSAttributeUseCategory.Required;

        [ContextProperty("Ограничение", "Constraint")]
        public XSConstraint Constraint { get; set; }

        #endregion

        #region Methods

        [ContextMethod("КлонироватьКомпоненту", "CloneComponent")]
        public IXSComponent CloneComponent(bool recursive = true) => throw new NotImplementedException();

        [ContextMethod("ОбновитьЭлементDOM", "UpdateDOMElement")]
        public void UpdateDOMElement() => throw new NotImplementedException();

        [ContextMethod("Содержит", "Contains")]
        public bool Contains(IXSComponent component) => Components.Contains(component);

        #endregion

        #region Constructors

        [ScriptConstructor(Name = "По умолчанию")]
        public static XSAttributeUse Constructor() => new XSAttributeUse();

        #endregion

        #endregion

        #region IXSComponent

        public void BindToContainer(IXSComponent rootContainer, IXSComponent container)
        {
            RootContainer = rootContainer;
            Container = container;
        }

        public XmlSchemaObject SchemaObject => _attributeDeclaration?.SchemaObject;

        #endregion
    }
}