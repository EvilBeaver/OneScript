/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Types;

namespace OneScript.StandardLibrary.NativeApi
{
    /// <summary>
    /// Фабрика, осуществляющая регистрацию библиотеки внешних 
    /// компонент Native API и создания экземпляров компонент.
    /// </summary>
    public class NativeApiFactory : NativeApiKernel
    {
        public static bool Register(string filepath, string identifier, ITypeManager typeManager)
        {
            if (_libraries.ContainsKey(identifier)) 
                return false;
            var library = new NativeApiLibrary(filepath, identifier, typeManager);
            if (library.Loaded) 
                _libraries.Add(identifier, library);
            return library.Loaded;
        }

        private static readonly Dictionary<string, NativeApiLibrary> _libraries = new Dictionary<string, NativeApiLibrary>();

        internal static void Initialize()
        {
            foreach (var item in _libraries) 
                item.Value.Dispose();
            _libraries.Clear();
        }

        [ScriptEngine.Machine.Contexts.ScriptConstructor]
        public static IValue Constructor(TypeActivationContext context)
        {
            var typeName = context.TypeName;
            var separator = new char[] { '.' };
            var names = typeName.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (names.Length == 3 && _libraries.TryGetValue(names[1], out NativeApiLibrary library))
                return library.CreateComponent(context.TypeManager, default, typeName, names[2]);
            throw new NotImplementedException();
        }
    }
}
