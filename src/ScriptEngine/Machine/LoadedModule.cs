/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Compiler;
using ScriptEngine.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    class LoadedModule
    {
        internal LoadedModule(ModuleImage image)
        {
            this.Code = image.Code.ToArray();
            this.EntryMethodIndex = image.EntryMethodIndex;
            this.MethodRefs = image.MethodRefs.ToArray();
            this.VariableRefs = image.VariableRefs.ToArray();
            this.Methods = image.Methods.ToArray();
            this.Constants = new IValue[image.Constants.Count];
            this.VariableFrameSize = image.VariableFrameSize;
            this.ExportedProperies = image.ExportedProperties.ToArray();
            this.ExportedMethods = image.ExportedMethods.ToArray();
            this.ModuleInfo = image.ModuleInfo;
            for (int i = 0; i < image.Constants.Count; i++)
            {
                var def = image.Constants[i];
                this.Constants[i] = ValueFactory.Parse(def.Presentation, def.Type);
            }
        }

        public int VariableFrameSize { get; private set; }
        public int EntryMethodIndex { get; private set; }
        public Command[] Code { get; private set;}
        public SymbolBinding[] VariableRefs { get; private set; }
        public SymbolBinding[] MethodRefs { get; private set; }
        public MethodDescriptor[] Methods { get; private set; }
        public IValue[] Constants { get; private set; }
        public ExportedSymbol[] ExportedProperies { get; private set; }
        public ExportedSymbol[] ExportedMethods { get; private set; }
        public ModuleInformation ModuleInfo { get; private set; }
    }

    
}
