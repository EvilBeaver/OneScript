/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Compilation;

namespace ScriptEngine.Libraries
{
    /// <summary>
    /// Информация о внешней библиотеке и её компонентах.
    /// </summary>
    public class ExternalLibraryInfo
    {
        public ExternalLibraryInfo(PackageInfo package)
        {
            Package = package;
        }
        
        public PackageInfo Package { get; }
        
        public List<UserAddedScript> Modules { get; } = new List<UserAddedScript>();
        public List<UserAddedScript> Classes { get; } = new List<UserAddedScript>();
        
        public void AddModule(string symbol, string filePath)
        {
            Modules.Add(new UserAddedScript { Symbol = symbol, FilePath = filePath });
        }
        
        public void AddClass(string symbol, string filePath)
        {
            Classes.Add(new UserAddedScript { Symbol = symbol, FilePath = filePath });
        }
    }
}

