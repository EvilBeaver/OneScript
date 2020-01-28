/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    /// <summary>
    /// Интерфейс для реализации элементов аннотации
    /// </summary>
    /// <see cref="XSAnnotation"/>
    /// <seealso cref="XSDocumentation"/>
    /// <seealso cref="XSAppInfo"/>
    internal interface IXSAnnotationItem : IXSComponent
    {
        string Source { get; set; }
    }
}
