/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using ScriptEngine.Hosting;

namespace ScriptEngine.HostedScript
{
    public class OneScriptLibraryOptions : OneScriptCoreOptions
    {
        public const string SYSTEM_LIBRARY_DIR = "lib.system";
        public const string ADDITIONAL_LIBRARIES = "lib.additional";
        
        public OneScriptLibraryOptions(KeyValueConfig config) : base(config)
        {
            SystemLibraryDir = config[SYSTEM_LIBRARY_DIR];

            var additionalDirsList = config[ADDITIONAL_LIBRARIES];
            if (additionalDirsList != null)
            {
                var addDirs = additionalDirsList.Split(';');
                AdditionalLibraries = new List<string>(addDirs);
            }
        }

        public string SystemLibraryDir { get; set; }
        
        public IEnumerable<string> AdditionalLibraries { get; set; }
    }
}