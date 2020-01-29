/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Xml.Schema;
using OneScript.StandardLibrary.XMLSchema.Collections;
using OneScript.StandardLibrary.XMLSchema.Enumerations;
using OneScript.StandardLibrary.XMLSchema.Objects;
using ScriptEngine.Machine;

namespace OneScript.StandardLibrary.XMLSchema.Interfaces
{
    public interface IXSComponent : IRuntimeContextInstance, IValue
    {
        #region OneScript

        XSAnnotation Annotation         { get; }
        XSComponentFixedList Components { get; }
        IXSComponent Container          { get; }
        IXSComponent RootContainer      { get; }
        Objects.XMLSchema Schema                { get; }
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
