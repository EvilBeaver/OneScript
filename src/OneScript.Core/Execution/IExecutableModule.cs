/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Contexts;
using OneScript.Sources;

namespace OneScript.Execution
{
    public interface IExecutableModule
    {
        IList<BslAnnotationAttribute> ModuleAttributes { get; }

        IList<BslFieldInfo> Fields { get; }
        
        IList<BslPropertyInfo> Properties { get; }
        
        IList<BslMethodInfo> Methods { get; }
        
        BslMethodInfo ModuleBody { get; }
        
        SourceCode Source { get; }
        
        const string BODY_METHOD_NAME = "$entry";
    }
}