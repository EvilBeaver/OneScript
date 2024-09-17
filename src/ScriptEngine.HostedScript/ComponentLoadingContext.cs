/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using OneScript.Commons;

namespace ScriptEngine.HostedScript
{
    internal class ComponentLoadingContext : AssemblyLoadContext
    {
        private static readonly IDictionary<AssemblyName, Assembly> _currentAssemblies =
            new Dictionary<AssemblyName, Assembly>(new AssemblyComparer());

        private readonly AssemblyDependencyResolver _resolver;

        public ComponentLoadingContext(string path)
        {
            _resolver = new AssemblyDependencyResolver(path);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            if (_currentAssemblies.TryGetValue(assemblyName, out var result))
                return result;
                
            var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }

        private class AssemblyComparer : IEqualityComparer<AssemblyName>
        {
            public bool Equals(AssemblyName x, AssemblyName y)
            {
                return AssemblyName.ReferenceMatchesDefinition(x, y);
            }

            public int GetHashCode(AssemblyName obj)
            {
                return obj.Name?.GetHashCode() ?? 0;
            }
        }

        private static void AddAssembly(Assembly asm)
        {
            _currentAssemblies.Add(asm.GetName(), asm);
        }
 
        static ComponentLoadingContext()
        {
            AppDomain.CurrentDomain.GetAssemblies().ForEach(AddAssembly);
        }
    }
}