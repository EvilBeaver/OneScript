/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using ScriptEngine.Machine;

namespace ScriptEngine.Persistence
{
    [Serializable]
    public class ModuleImage
    {
        public VariablesFrame Variables { get; } = new VariablesFrame();
        public int EntryMethodIndex { get; set; } = -1;
        public IList<Command> Code { get; set; } = new List<Command>();
        public IList<ExternalSymbol> VariableRefs { get; set; } = new List<ExternalSymbol>();
        public IList<ExternalSymbol> MethodRefs { get; set; } = new List<ExternalSymbol>();
        public IList<MethodDescriptor> Methods { get; set; } = new List<MethodDescriptor>();
        public IList<ConstDefinition> Constants { get; set; } = new List<ConstDefinition>();
        public IList<ExportedSymbol> ExportedProperties { get; set; } = new List<ExportedSymbol>();
        public IList<ExportedSymbol> ExportedMethods { get; set; } = new List<ExportedSymbol>();
        public int LoadAddress { get; set; }
        public IList<AnnotationDefinition> Annotations { get; set; } = new List<AnnotationDefinition>();
        public string SourceCodeOrigin { get; set; }
    }
}
