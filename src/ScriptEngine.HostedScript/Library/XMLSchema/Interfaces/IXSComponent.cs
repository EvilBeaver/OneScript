using System;
using System.Collections.Generic;
using System.Linq;
using ScriptEngine.Machine;
using System.Xml.Schema;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    internal interface IXSComponent : IValue
    {
        #region OneScript

        XSAnnotation Annotation         { get; }
        XSComponentFixedList Components { get; }
        IValue Container                { get; }
        IValue RootContainer            { get; }
        XMLSchema Schema                { get; }
        XSComponentType ComponentType   { get; }
        //DOMElement

        IValue CloneComponent(IValue recursive = null);
        void UpdateDOMElement();
        bool Contains(IValue component);

        #endregion

        XmlSchemaObject SchemaObject { get; }
        void BindToContainer(IXSComponent RootContainer, IXSComponent Container);

    }
}
