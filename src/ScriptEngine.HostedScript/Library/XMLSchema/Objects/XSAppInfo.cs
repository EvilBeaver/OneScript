/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Diagnostics.Contracts;
using System.Xml.Schema;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("ИнформацияДляПриложенияXS", "XSAppInfo")]
    public class XSAppInfo : AutoContext<XSAppInfo>, IXSAnnotationItem
    {

        private readonly XmlSchemaAppInfo _appInfo;

        private XSAppInfo() => _appInfo = new XmlSchemaAppInfo();

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
        public XSComponentType ComponentType => XSComponentType.AppInfo;

        [ContextProperty("Источник", "Source")]
        public string Source
        {
            get => _appInfo.Source;
            set => _appInfo.Source = value;
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
        public static XSAppInfo Constructor() => new XSAppInfo();

        #endregion

        #endregion

        #region IXComponent

        XmlSchemaObject IXSComponent.SchemaObject => _appInfo;

        void IXSComponent.BindToContainer(IXSComponent rootContainer, IXSComponent container)
        {
            Contract.Requires(container is XSAnnotation);
            RootContainer = rootContainer;
            Container = container;
        }

        #endregion
    }
}
