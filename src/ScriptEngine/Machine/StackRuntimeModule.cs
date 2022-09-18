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
using OneScript.Execution;
using OneScript.Sources;
using OneScript.Values;

namespace ScriptEngine.Machine
{
    public class StackRuntimeModule : IExecutableModule
    {
        public StackRuntimeModule(Type ownerType)
        {
            ClassType = ownerType;
        }

        public Type ClassType { get; }

        public int LoadAddress { get; set; } = -1;

        public int EntryMethodIndex { get; set; } = -1;

        public List<BslPrimitiveValue> Constants { get; } = new List<BslPrimitiveValue>();
        
        public IList<SymbolBinding> VariableRefs { get; } = new List<SymbolBinding>();
        
        public IList<SymbolBinding> MethodRefs { get; } = new List<SymbolBinding>();

        #region IExecutableModule members

        public BslMethodInfo ModuleBody
        {
            get
            {
                if (EntryMethodIndex == -1)
                    return null;

                return Methods[MethodRefs[EntryMethodIndex].MemberNumber];
            }
        }
        
        public IList<BslAnnotationAttribute> ModuleAttributes { get; } = new List<BslAnnotationAttribute>();
        
        public IList<BslFieldInfo> Fields { get; } = new List<BslFieldInfo>();
        
        public IList<BslPropertyInfo> Properties { get; } = new List<BslPropertyInfo>();

        public IList<BslMethodInfo> Methods { get; } = new List<BslMethodInfo>();

        public IList<Command> Code { get; } = new List<Command>(512);

        public SourceCode Source { get; set; }

        #endregion
    }
}