/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine;
using System.Xml.Schema;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    public interface IXSComponent : IRuntimeContextInstance, IValue
    {
        #region OneScript

        XSAnnotation Annotation         { get; }
        XSComponentFixedList Components { get; }
        IXSComponent Container          { get; }
        IXSComponent RootContainer      { get; }
        XMLSchema Schema                { get; }
        XSComponentType ComponentType   { get; }
        //DOMElement

        IXSComponent CloneComponent(bool recursive = true);
        void UpdateDOMElement();
        bool Contains(IXSComponent component);

        #endregion

        XmlSchemaObject SchemaObject { get; }
        void BindToContainer(IXSComponent RootContainer, IXSComponent Container);

    }
}
