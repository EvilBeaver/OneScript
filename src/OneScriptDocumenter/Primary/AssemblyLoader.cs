/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Reflection;

namespace OneScriptDocumenter.Primary
{
    public class AssemblyLoader : IDisposable
    {
        //private readonly HashSet<AssemblyLoadContext> _loadContexts = new HashSet<AssemblyLoadContext>();
        
        public Dictionary<string, Assembly> LoadAssemblies(IReadOnlyCollection<string> assemblyPaths)
        {
            var assemblies = new Dictionary<string, Assembly>();
            foreach (var assemblyPath in assemblyPaths)
            {
                var context = new DocumentableLibraryLoadContext(assemblyPath);
                //_loadContexts.Add(context);
                var assembly = context.LoadFromAssemblyPath(assemblyPath);
                assemblies.Add(assembly.GetName().Name!, assembly);
            }

            return assemblies;
        }

        public void Dispose()
        {
            // foreach (var loadContext in _loadContexts)
            // {
            //     loadContext.Unload();
            // }
        }
    }
}