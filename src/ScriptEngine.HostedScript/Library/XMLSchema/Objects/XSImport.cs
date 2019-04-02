/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Xml.Schema;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{

    /// <summary>
    /// Описывает тег "import" для схемы XML.
    /// </summary>
    /// <see cref="System.Xml.Schema.XmlSchemaImport"/> 
    [ContextClass("ИмпортXS", "XSImport")]
    public class XSImport : AutoContext<XSImport>, IXSDirective
    {
        private readonly XmlSchemaImport _import;

        private XSImport() => _import = new XmlSchemaImport();

        #region OneScript

        #region Properties

        [ContextProperty("Аннотация", "Annotation")]
        public XSAnnotation Annotation => null;

        [ContextProperty("Компоненты", "Components")]
        public XSComponentFixedList Components => null;

        [ContextProperty("Контейнер", "Container")]
        public IXSComponent Container { get; private set; }

        [ContextProperty("КорневойКонтейнер", "RootContainer")]
        public IXSComponent RootContainer { get; private set; }

        [ContextProperty("Схема", "Schema")]
        public XMLSchema Schema => RootContainer.Schema;

        [ContextProperty("ТипКомпоненты", "ComponentType")]
        public XSComponentType ComponentType => XSComponentType.Import;

        [ContextProperty("РазрешеннаяСхема", "ResolvedSchema")]
        public XMLSchema ResolvedSchema
        {
            get => ResolvedSchema;
            set => ResolvedSchema = value;
        }

        [ContextProperty("РасположениеСхемы", "SchemaLocation")]
        public string SchemaLocation
        {
            get => _import.SchemaLocation;
            set => _import.SchemaLocation = value;
        }

        [ContextProperty("Префикс", "Prefix")]
        public string Prefix { get; set; }

        [ContextProperty("ПространствоИмен", "Namespace")]
        public string Namespace
        {
            get => _import.Namespace;
            set => _import.Namespace = value;
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
        public static XSImport Constructor() => new XSImport();

        #endregion

        #endregion

        #region IXSComponent

        XmlSchemaObject IXSComponent.SchemaObject => _import;

        void IXSComponent.BindToContainer(IXSComponent rootContainer, IXSComponent container)
        {
            RootContainer = rootContainer;
            Container = container;
        }

        #endregion

    }
}
