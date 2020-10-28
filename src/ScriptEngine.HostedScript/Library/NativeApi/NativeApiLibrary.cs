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
    class NativeApiLibrary : NativeApiKernel
    {
        private delegate IntPtr GetClassNames();

        public static bool Register(String filepath, String identifier)
        {
            if (_libraries.ContainsKey(identifier)) 
                return false;
            var instance = new NativeApiLibrary(filepath, identifier);
            if (instance.Loaded) 
                _libraries.Add(identifier, instance);
            return instance.Loaded;
        }

        private static Dictionary<string, NativeApiLibrary> _libraries = new Dictionary<string, NativeApiLibrary>();

        private List<NativeApiComponent> _components = new List<NativeApiComponent>();

        private readonly String _tempfile;

        internal static void Initialize()
        {
            foreach (var item in _libraries) 
                item.Value.Dispose();
            _libraries.Clear();
        }

        public NativeApiLibrary(String filepath, String identifier)
        {
            using (FileStream stream = File.OpenRead(filepath))
            {
                if (NativeApiLoader.IsZip(stream))
                {
                    _tempfile = Path.GetTempFileName();
                    NativeApiLoader.Extract(stream, _tempfile);
                    Module = LoadLibrary(_tempfile);
                }
                else 
                    Module = LoadLibrary(filepath);
            }
            if (Loaded) RegisterComponents(identifier);
        }

        public void Dispose()
        {
            foreach (var component in _components)
                component.Dispose();
            if (Loaded && FreeLibrary(Module))
                if (!String.IsNullOrEmpty(_tempfile))
                    try { File.Delete(_tempfile); } catch (Exception) { }
        }

        public IntPtr Module { get; private set; } = IntPtr.Zero;

        private Boolean Loaded
        {
            get => Module != IntPtr.Zero;
        }

        private void RegisterComponents(String identifier)
        {
            var funcPtr = GetProcAddress(Module, "GetClassNames");
            if (funcPtr == IntPtr.Zero) 
                throw new RuntimeException("В библиотеке внешних компонент не обнаружена функция: GetClassNames()");
            var namesPtr = Marshal.GetDelegateForFunctionPointer<GetClassNames>(funcPtr)();
            if (namesPtr == IntPtr.Zero) 
                throw new RuntimeException("Не удалось получить список компонент в составе библиотеки");
            var separator = new char[] { '|' };
            var names = NativeApiProxy.Str(namesPtr).Split(separator, StringSplitOptions.RemoveEmptyEntries);
            foreach (String name in names)
                TypeManager.RegisterType($"AddIn.{identifier}.{name}", typeof(NativeApiLibrary));
        }

        [Machine.Contexts.ScriptConstructor(ParametrizeWithClassName = true)]
        public static IValue Constructor(String typeName)
        {
            var separator = new char[] { '.' };
            var names = typeName.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (names.Length == 3 && _libraries.TryGetValue(names[1], out NativeApiLibrary library))
            {
                NativeApiComponent comp = new NativeApiComponent(library, typeName, names[2]);
                library._components.Add(comp);
                return ValueFactory.Create(comp);
            }
            throw new NotImplementedException();
        }
    }
}
