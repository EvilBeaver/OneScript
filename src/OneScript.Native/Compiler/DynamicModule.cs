/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Language;
using OneScript.Native.Runtime;

namespace OneScript.Native.Compiler
{
    public class DynamicModule
    {
        public IList<BslNativeFieldInfo> Fields { get; } = new List<BslNativeFieldInfo>();
        
        public IList<BslNativeMethodInfo> Methods { get; } = new List<BslNativeMethodInfo>();
        public ModuleInformation ModuleInformation { get; set; }
    }
}