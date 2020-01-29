/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.StandardLibrary.XMLSchema.Interfaces
{
    public interface IXSDirective : IXSComponent
    {
        Objects.XMLSchema ResolvedSchema { get; set; }
        string SchemaLocation { get; set; }
    }
}
