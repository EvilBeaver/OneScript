/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using OneScript.Contexts;

namespace OneScriptDocumenter.Primary
{
    public class DocumentableLibraryLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _dependencyResolver;
        private readonly HashSet<string> _assembliesOfCurrentApp;
        private readonly string _assemblyFile;

        public DocumentableLibraryLoadContext(string assemblyFile) : base(isCollectible: false)
        {
            _assemblyFile = assemblyFile;
            _dependencyResolver = new AssemblyDependencyResolver(assemblyFile);

            // Мы должны не грузить в данный контекст библиотеку Core
            // Тогда она будет загружена из контекста AppDomain
            // и сравнение типов у атрибутов разметки будет работать.
            // В противном случае сравнение ContextClassAttribute из разных DLL будет давать false на равенство
            var coreAsm = typeof(ContextClassAttribute).Assembly;
            _assembliesOfCurrentApp = AppDomain.CurrentDomain.GetAssemblies()
                .First(a => a.FullName == coreAsm.FullName)
                .GetReferencedAssemblies()
                .Select(a => a.FullName)
                .ToHashSet();
            _assembliesOfCurrentApp.Add(coreAsm.FullName);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            if (_assembliesOfCurrentApp.Contains(assemblyName.FullName))
            {
                return null;
            }

            var assemblyPath = _dependencyResolver.ResolveAssemblyToPath(assemblyName);

            if (assemblyPath != null) 
                return LoadFromAssemblyPath(assemblyPath);

            if (assemblyName.FullName.StartsWith("System.") || assemblyName.FullName.StartsWith("Microsoft."))
            {
                return Assembly.Load(assemblyName);
            }
            
            ConsoleLogger.Error($"Error loading {_assemblyFile}. Dependency {assemblyName.FullName} probably not in dll directory");
            return null;

        }
        
        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string libraryPath = _dependencyResolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }
    }
}