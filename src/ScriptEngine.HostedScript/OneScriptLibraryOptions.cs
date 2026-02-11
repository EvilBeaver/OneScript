/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using ScriptEngine.Hosting;

namespace ScriptEngine.HostedScript
{
    public class OneScriptLibraryOptions : OneScriptCoreOptions
    {
        public const string SYSTEM_LIBRARY_DIR = "lib.system";
        public const string ADDITIONAL_LIBRARIES = "lib.additional";
        
        public OneScriptLibraryOptions(KeyValueConfig config) : base(config)
        {
            SystemLibraryDir = config.GetEntry(SYSTEM_LIBRARY_DIR)?.ResolvePath();
            AdditionalLibraries = config.GetEntry(ADDITIONAL_LIBRARIES)?.ResolvePathList(';').ToList();
        }

        public string SystemLibraryDir { get; set; }
        
        public IEnumerable<string> AdditionalLibraries { get; set; }
    }
}
