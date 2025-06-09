/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Contexts;
using OneScript.Sources;

namespace OneScript.Execution
{
    public interface IExecutableModule
    {
        IList<BslAnnotationAttribute> ModuleAttributes { get; }

        IList<BslScriptFieldInfo> Fields { get; }
        
        IList<BslScriptPropertyInfo> Properties { get; }
        
        IList<BslScriptMethodInfo> Methods { get; }
        
        BslScriptMethodInfo ModuleBody { get; }
        
        SourceCode Source { get; }
        
        IDictionary<Type, object> Interfaces { get; }
        
        const string BODY_METHOD_NAME = "$entry";
    }
}