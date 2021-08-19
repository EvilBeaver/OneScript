/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using OneScript.Contexts;
using OneScript.Language;
using OneScript.Sources;
using ScriptEngine.Compiler;

namespace ScriptEngine.Machine
{
    public class LoadedModule: ExecutableModule
    {
        public VariablesFrame Variables { get; }
        
        public ExportedSymbol[] ExportedProperies { get; }
        public ExportedSymbol[] ExportedMethods { get; }
        public AnnotationDefinition[] Annotations { get; }
        public ModuleInformation ModuleInfo { get; }

    }

    
}
