/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using OneScript.Compilation.Binding;
using OneScript.Contexts;
using OneScript.Language;
using OneScript.Sources;
using ScriptEngine.Machine;

namespace ScriptEngine
{
    [Serializable]
    public class ModuleImage
    {
        public VariablesFrame Variables { get; } = new VariablesFrame();
        public int EntryMethodIndex { get; set; } = -1;
        public IList<Command> Code { get; set; } = new List<Command>();
        public IList<SymbolBinding> VariableRefs { get; set; } = new List<SymbolBinding>();
        public IList<SymbolBinding> MethodRefs { get; set; } = new List<SymbolBinding>();
        public IList<MethodDescriptor> Methods { get; set; } = new List<MethodDescriptor>();
        public IList<ConstDefinition> Constants { get; set; } = new List<ConstDefinition>();
        public IList<ExportedSymbol> ExportedProperties { get; set; } = new List<ExportedSymbol>();
        public IList<ExportedSymbol> ExportedMethods { get; set; } = new List<ExportedSymbol>();
        public int LoadAddress { get; set; }
        
        [Obsolete("Use Source")]
        public ModuleInformation ModuleInfo { get; set; }

        [NonSerialized] private SourceCode _sourceBackField;

        public SourceCode Source
        {
            get => _sourceBackField;
            set => _sourceBackField = value;
        }
        public IList<AnnotationDefinition> Annotations { get; set; } = new List<AnnotationDefinition>();
    }

    [Serializable]
    public struct MethodDescriptor
    {
        public MethodSignature Signature;
        public VariablesFrame Variables;
        public int EntryPoint;
        
        [NonSerialized]
        public BslMethodInfo MethodInfo;
    }

    [Serializable]
    public struct ExportedSymbol
    {
        public string SymbolicName;
        public int Index;
    }
}
