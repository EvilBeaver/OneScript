/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using OneScript.Contexts;
using OneScript.Execution;

namespace OneScript.Sources
{
    public sealed class EmptyModule : IExecutableModule
    {
        public static readonly IExecutableModule Instance = new EmptyModule(); 
        
        private EmptyModule()
        {
            ModuleBody = BslScriptMethodInfo.Create();
            Source = SourceCodeBuilder.Create().FromString("").Build();
        }

        public IList<BslAnnotationAttribute> ModuleAttributes => Array.Empty<BslAnnotationAttribute>();
        public IList<BslScriptFieldInfo> Fields => Array.Empty<BslScriptFieldInfo>();

        public IList<BslScriptPropertyInfo> Properties => Array.Empty<BslScriptPropertyInfo>();
        public IList<BslScriptMethodInfo> Methods => Array.Empty<BslScriptMethodInfo>();
        public BslScriptMethodInfo ModuleBody { get; }
        public SourceCode Source { get; }

        public IDictionary<Type, object> Interfaces => new ReadOnlyDictionary<Type, object>(new Dictionary<Type, object>());
    }
}