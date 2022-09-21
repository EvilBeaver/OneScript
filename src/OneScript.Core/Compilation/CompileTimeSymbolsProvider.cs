/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using OneScript.Compilation.Binding;

namespace OneScript.Compilation
{
    public class CompileTimeSymbolsProvider
    {
        private delegate void Filler(CompileTimeSymbolsProvider provider, SymbolScope scope);
        
        private readonly ConcurrentDictionary<Type, SymbolProvider> _providers =
            new ConcurrentDictionary<Type, SymbolProvider>();

        public IModuleSymbolsProvider Get<T>()
        {
            return Get(typeof(T));
        }
        
        public IModuleSymbolsProvider Get(Type type)
        {
            return _providers.GetOrAdd(type, CreateProvider);
        }

        private SymbolProvider CreateProvider(Type type)
        {
            var filler = FindFillerMethod(type);
            return new SymbolProvider(this, filler);
        }

        private static Filler FindFillerMethod(Type type)
        {
            var filler = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttribute<SymbolsProviderAttribute>() != default)
                .Where(IsFiller)
                .Select(m => (Filler)m.CreateDelegate(typeof(Filler)))
                .SingleOrDefault() ?? DoNothing;

            return filler;
        }

        private static void DoNothing(CompileTimeSymbolsProvider provider, SymbolScope scope)
        {
        }

        private static bool IsFiller(MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();
            return parameters.Length == 2
                   && parameters[0].ParameterType == typeof(CompileTimeSymbolsProvider)
                   && parameters[1].ParameterType == typeof(SymbolScope);
        }

        private class SymbolProvider : IModuleSymbolsProvider
        {
            private CompileTimeSymbolsProvider _provider;
            private Filler _filler;

            public SymbolProvider(CompileTimeSymbolsProvider provider, Filler filler)
            {
                _provider = provider;
                _filler = filler;
            }

            public void FillSymbols(SymbolScope moduleScope)
            {
                _filler(_provider, moduleScope);
            }
        }
    }
}
