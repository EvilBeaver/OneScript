/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Xml.Schema;
using OneScript.StandardLibrary.Xml;
using OneScript.StandardLibrary.XMLSchema.Collections;
using OneScript.StandardLibrary.XMLSchema.Enumerations;
using OneScript.StandardLibrary.XMLSchema.Interfaces;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.XMLSchema.Objects
{
    [ContextClass("ОпределениеОграниченияИдентичностиXS", "XSIdentityConstraintDefinition")]
    public class XSIdentityConstraintDefinition : AutoContext<XSIdentityConstraintDefinition>, IXSAnnotated, IXSNamedComponent
    {
        private XmlSchemaIdentityConstraint _constraint;
        private XSAnnotation _annotation;
        private XSIdentityConstraintCategory _category;
        private XMLExpandedName _referencedKeyName;

        private XSIdentityConstraintDefinition()
        {
            _constraint = new XmlSchemaKey();
            _category = XSIdentityConstraintCategory.Key;

            Components = new XSComponentFixedList();
            Fields = new XSComponentList();
            Fields.Inserted += Fields_Inserted;
            Fields.Cleared += Fields_Cleared;
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
                XSAnnotation.SetComponentAnnotation(_annotation, _constraint);
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
        public XSComponentType ComponentType => XSComponentType.IdentityConstraintDefinition;

        [ContextProperty("URIПространстваИмен", "NamespaceURI")]
        public string NamespaceURI => _constraint.SourceUri;

        [ContextProperty("Имя", "Name")]
        public string Name
        {
            get => _constraint.Name;
            set => _constraint.Name = value;
        }

        [ContextProperty("ИмяСсылочногоКлюча", "ReferencedKeyName")]
        public XMLExpandedName ReferencedKeyName
        {
            get => _referencedKeyName;
            set
            {
                _referencedKeyName = value;

                if (_constraint is XmlSchemaKeyref keyref)
                    keyref.Refer = _referencedKeyName?.NativeValue;
                else
                    throw RuntimeException.InvalidArgumentValue();
            }
        }

        [ContextProperty("Категория", "Category")]
        public XSIdentityConstraintCategory Category
        {
            get => _category;
            set
            {
                _category = value;
                switch(_category)
                {
                    case XSIdentityConstraintCategory.Key:
                        _constraint = new XmlSchemaKey();
                        break;

                    case XSIdentityConstraintCategory.KeyRef:
                        _constraint = new XmlSchemaKeyref();
                        break;

                    default:
                        _constraint = new XmlSchemaUnique();
                        break;                   
                }
            }
        }
        
        [ContextProperty("Поля", "Fields")]
        public XSComponentList Fields { get; }

        [ContextProperty("Селектор", "Selector")]
        public XSXPathDefinition Selector { get; set; }

        [ContextProperty("СсылочныйКлюч", "ReferencedKey")]
        public XSIdentityConstraintDefinition ReferencedKey { get;  }
        
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
        public static XSIdentityConstraintDefinition Constructor() => new XSIdentityConstraintDefinition();

        #endregion

        #endregion

        #region IXSComponent

        void IXSComponent.BindToContainer(IXSComponent rootContainer, IXSComponent container)
        {
            RootContainer = rootContainer;
            Container = container;
        }

        XmlSchemaObject IXSComponent.SchemaObject => _constraint;

        #endregion

        #region XSComponentListEvents

        private void Fields_Inserted(object sender, XSComponentListEventArgs e)
        {
            var component = e.Component;

            if (!(component is XSXPathDefinition))
                throw RuntimeException.InvalidArgumentType();

            component.BindToContainer(RootContainer, this);
            Components.Add(component);
            _constraint.Fields.Add(component.SchemaObject);
        }

        private void Fields_Cleared(object sender, EventArgs e)
        {
            Components.Clear();
            _constraint.Fields.Clear();
        }

        #endregion
    }
}
