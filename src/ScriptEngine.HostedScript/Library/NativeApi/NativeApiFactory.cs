/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace ScriptEngine.HostedScript.Library.NativeApi
{
    class NativeApiFactory : NativeApiKernel
    {
        public static bool Register(String filepath, String identifier)
        {
            if (_libraries.ContainsKey(identifier)) 
                return false;
            var library = new NativeApiLibrary(filepath, identifier);
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

        [Machine.Contexts.ScriptConstructor(ParametrizeWithClassName = true)]
        public static IValue Constructor(String typeName)
        {
            var separator = new char[] { '.' };
            var names = typeName.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (names.Length == 3 && _libraries.TryGetValue(names[1], out NativeApiLibrary library))
                return library.CreateComponent(typeName, names[2]);
            throw new NotImplementedException();
        }
    }
}
