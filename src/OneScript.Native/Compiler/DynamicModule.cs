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
using OneScript.Execution;
using OneScript.Sources;

namespace OneScript.Native.Compiler
{
    public class DynamicModule : IExecutableModule
    {
        public IList<BslAnnotationAttribute> ModuleAttributes { get; } = new List<BslAnnotationAttribute>();
        
        public IList<BslFieldInfo> Fields { get; } = new List<BslFieldInfo>();

        public IList<BslPropertyInfo> Properties { get; } = new List<BslPropertyInfo>();

        public IList<BslMethodInfo> Methods { get; } = new List<BslMethodInfo>();

        public BslMethodInfo ModuleBody => Methods.FirstOrDefault(x => x.Name == IExecutableModule.BODY_METHOD_NAME);

        public SourceCode Source { get; set; }

        public IDictionary<Type, object> Interfaces { get; } = new Dictionary<Type, object>();
    }
}