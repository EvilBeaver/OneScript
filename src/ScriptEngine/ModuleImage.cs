/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using ScriptEngine.Machine;
using ScriptEngine.Environment;

namespace ScriptEngine
{
    [Serializable]
    public class ModuleImage
    {
        public ModuleImage()
        {
            EntryMethodIndex = -1;
            Code = new List<Command>();
            VariableRefs = new List<SymbolBinding>();
            MethodRefs = new List<SymbolBinding>();
            Methods = new List<MethodDescriptor>();
            Constants = new List<ConstDefinition>();
            ExportedProperties = new List<ExportedSymbol>();
            ExportedMethods = new List<ExportedSymbol>();
            Variables = new VariablesFrame();
            Annotations = new List<AnnotationDefinition>();
        }

        public VariablesFrame Variables { get; }
        public int EntryMethodIndex { get; set; }
        public IList<Command> Code { get; set; }
        public IList<SymbolBinding> VariableRefs { get; set; }
        public IList<SymbolBinding> MethodRefs { get; set; }
        public IList<MethodDescriptor> Methods { get; set; }
        public IList<ConstDefinition> Constants { get; set; }
        public IList<ExportedSymbol> ExportedProperties { get; set; }
        public IList<ExportedSymbol> ExportedMethods { get; set; }
        public int LoadAddress { get; set; }
        public ModuleInformation ModuleInfo { get; set; }
        public IList<AnnotationDefinition> Annotations { get; set; }
        
        public const string BODY_METHOD_NAME = "$entry";
    }

    [Serializable]
    public struct MethodDescriptor
    {
        public MethodInfo Signature;
        public VariablesFrame Variables;
        public int EntryPoint;
    }

    [Serializable]
    public struct ExportedSymbol
    {
        public string SymbolicName;
        public int Index;
    }
}
