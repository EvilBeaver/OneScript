/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
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

        public int EntryMethodIndex { get; set; } = -1;

        public List<BslPrimitiveValue> Constants { get; } = new List<BslPrimitiveValue>();
        
        internal IList<ModuleSymbolBinding> VariableRefs { get; } = new List<ModuleSymbolBinding>();
        
        internal IList<ModuleSymbolBinding> MethodRefs { get; } = new List<ModuleSymbolBinding>();
        
        #region IExecutableModule members

        public BslScriptMethodInfo ModuleBody
        {
            get
            {
                if (EntryMethodIndex == -1)
                    return null;

                return Methods[MethodRefs[EntryMethodIndex].MemberNumber];
            }
        }
        
        public IList<BslAnnotationAttribute> ModuleAttributes { get; } = new List<BslAnnotationAttribute>();
        
        public IList<BslScriptFieldInfo> Fields { get; } = new List<BslScriptFieldInfo>();
        
        public IList<BslScriptPropertyInfo> Properties { get; } = new List<BslScriptPropertyInfo>();

        public IList<BslScriptMethodInfo> Methods { get; } = new List<BslScriptMethodInfo>();

        public IList<Command> Code { get; } = new List<Command>(512);

        public SourceCode Source { get; set; }
        
        public IDictionary<Type, object> Interfaces { get; } = new Dictionary<Type, object>();

        #endregion
    }
}