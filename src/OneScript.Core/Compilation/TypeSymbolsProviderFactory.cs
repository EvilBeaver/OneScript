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
    /// <summary>
    /// Фабрика провайдеров дополнительных внешних символов для класса при компиляции.
    /// </summary>
    public class TypeSymbolsProviderFactory
    {
        private delegate void Filler(TypeSymbolsProviderFactory providerFactory, SymbolScope scope);
        
        private readonly ConcurrentDictionary<Type, SymbolProvider> _providers =
            new ConcurrentDictionary<Type, SymbolProvider>();

        public ITypeSymbolsProvider Get<T>()
        {
            return Get(typeof(T));
        }
        
        public ITypeSymbolsProvider Get(Type type)
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

        private static void DoNothing(TypeSymbolsProviderFactory providerFactory, SymbolScope scope)
        {
        }

        private static bool IsFiller(MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();
            return parameters.Length == 2
                   && parameters[0].ParameterType == typeof(TypeSymbolsProviderFactory)
                   && parameters[1].ParameterType == typeof(SymbolScope);
        }

        private class SymbolProvider : ITypeSymbolsProvider
        {
            private readonly TypeSymbolsProviderFactory _providerFactory;
            private readonly Filler _filler;

            public SymbolProvider(TypeSymbolsProviderFactory providerFactory, Filler filler)
            {
                _providerFactory = providerFactory;
                _filler = filler;
            }

            public void FillSymbols(SymbolScope moduleScope)
            {
                _filler(_providerFactory, moduleScope);
            }
        }
    }
}
