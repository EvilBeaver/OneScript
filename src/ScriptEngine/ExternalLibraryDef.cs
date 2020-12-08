/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace ScriptEngine
{
    public class ExternalLibraryDef
    {
        public ExternalLibraryDef(string name)
        {
            LibraryName = name;
        }
        
        public string LibraryName { get; }
        
        public IList<UserAddedScript> Classes { get; } = new List<UserAddedScript>();
        public IList<UserAddedScript> Modules { get; } = new List<UserAddedScript>();

        public UserAddedScript AddClass(string identifier, string filePath, ModuleImage image = null)
        {
            var item = new UserAddedScript
            {
                Type = UserAddedScriptType.Class,
                Image = image,
                Symbol = identifier,
                FilePath = filePath
            };
            
            Classes.Add(item);

            return item;
        }
        
        public UserAddedScript AddModule(string identifier, string filePath, ModuleImage image = null)
        {
            var item = new UserAddedScript
            {
                Type = UserAddedScriptType.Module,
                Image = image,
                Symbol = identifier,
                FilePath = filePath
            };
            
            Modules.Add(item);

            return item;
        }
    }
    
    [Serializable]
    public class UserAddedScript
    {
        public UserAddedScriptType Type;
        public ModuleImage Image;
        public string Symbol;
        public int InjectOrder;
        
        [NonSerialized]
        public string FilePath;
        
        public string ModuleName()
        {
            return Image.ModuleInfo.ModuleName;
        }
    }

    [Serializable]
    public enum UserAddedScriptType
    {
        Module,
        Class
    }
}