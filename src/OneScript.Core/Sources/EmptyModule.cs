/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Contexts;

namespace OneScript.Sources
{
    public sealed class EmptyModule : IExecutableModule
    {
        public static IExecutableModule Instance = new EmptyModule(); 
        
        private EmptyModule()
        {
            ModuleBody = BslScriptMethodInfo.Create();
            Source = SourceCodeBuilder.Create().FromString("").Build();
        }

        public IList<BslAnnotationAttribute> ModuleAttributes => Array.Empty<BslAnnotationAttribute>();
        public IList<BslFieldInfo> Fields => Array.Empty<BslFieldInfo>();

        public IList<BslPropertyInfo> Properties => Array.Empty<BslPropertyInfo>();
        public IList<BslMethodInfo> Methods => Array.Empty<BslMethodInfo>();
        public BslMethodInfo ModuleBody { get; }
        public SourceCode Source { get; }
    }
}