/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Types;
using OneScript.Contexts;
using ScriptEngine;
using ScriptEngine.Machine;

namespace OneScript.StandardLibrary.NativeApi
{
    /// <summary>
    /// Фабрика, осуществляющая регистрацию библиотеки внешних 
    /// компонент Native API и создания экземпляров компонент.
    /// </summary>
    class NativeApiFactory : IEngineLifetime
    {
        private readonly object _sync = new object();
        private readonly Dictionary<string, NativeApiLibrary> _libraries = new Dictionary<string, NativeApiLibrary>();
        private bool _disposed;

        public bool Register(string filepath, string identifier, ITypeManager typeManager)
        {
            lock (_sync)
            {
                ObjectDisposedException.ThrowIf(_disposed, this);

                if (_libraries.ContainsKey(identifier))
                    return true;

                var library = new NativeApiLibrary(filepath, identifier, typeManager);
                if (library.Loaded)
                    _libraries.Add(identifier, library);
                return library.Loaded;
            }
        }

        public void Dispose()
        {
            lock (_sync)
            {
                if (_disposed)
                    return;

                foreach (var item in _libraries)
                    item.Value.Dispose();

                _libraries.Clear();
                _disposed = true;
            }
        }

        [ScriptConstructor]
        public static IValue Constructor(TypeActivationContext context)
        {
            var factory = context.Services.Resolve<NativeApiFactory>();
            var typeName = context.TypeName;
            var separator = new char[] { '.' };
            var names = typeName.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (names.Length == 3 && factory.TryGetLibrary(names[1], out NativeApiLibrary library))
                return library.CreateComponent(context.TypeManager, default, typeName, names[2]);
            throw new NotImplementedException();
        }

        private bool TryGetLibrary(string identifier, out NativeApiLibrary library)
        {
            lock (_sync)
            {
                return _libraries.TryGetValue(identifier, out library);
            }
        }
    }
}
