/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using ScriptEngine.Environment;

namespace ScriptEngine.HostedScript
{
    public class FileSystemDependencyResolver : IDependencyResolver
    {
        public const string PREDEFINED_LOADER_FILE = "package-loader.os";
        
        private readonly List<Library> _libs = new List<Library>();
        private LibraryLoader _defaultLoader;

        #region Private classes

        private class Library
        {
            public string id;
            public ProcessingState state;
            public LibraryLoader customLoader;
        }

        private enum ProcessingState
        {
            Discovered,
            Processed
        }

        #endregion

        public IList<string> SearchDirectories { get;} = new List<string>();

        public string LibraryRoot => SearchDirectories.FirstOrDefault();
        
        private ScriptingEngine Engine { get; set; }
        
        public ExternalLibraryDef Resolve(ModuleInformation module, string libraryName)
        {
            throw new NotImplementedException();
        }

        public void SetEngine(ScriptingEngine engine)
        {
            Engine = engine;
        }
        
    }
}