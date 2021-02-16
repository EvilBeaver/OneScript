/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using ScriptEngine.Machine;

namespace OneScript.StandardLibrary.NativeApi
{
    /// <summary>
    /// Класс, ассоциированный с экземпляром библиотеки внешних компонент 
    /// Native API и осуществляющий непосредственное создание экземпляра компоненты.
    /// </summary>
    public class NativeApiLibrary : NativeApiKernel
    {
        private delegate IntPtr GetClassNames();

        private readonly List<NativeApiComponent> _components = new List<NativeApiComponent>();

        private readonly String _tempfile;

        public NativeApiLibrary(string filepath, string identifier, ITypeManager typeManager)
        {
            using (var stream = File.OpenRead(filepath))
            {
                if (NativeApiPackage.IsZip(stream))
                {
                    _tempfile = Path.GetTempFileName();
                    NativeApiPackage.Extract(stream, _tempfile);
                    Module = LoadLibrary(_tempfile);
                }
                else 
                    Module = LoadLibrary(filepath);
            }
            if (Loaded) 
                RegisterComponents(identifier, typeManager);
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

        public Boolean Loaded
        {
            get => Module != IntPtr.Zero;
        }

        private void RegisterComponents(string identifier, ITypeManager typeManager)
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
                typeManager.RegisterType($"AddIn.{identifier}.{name}", default, typeof(NativeApiFactory));
        }

        public IValue CreateComponent(ITypeManager typeManager, object host, String typeName, String componentName)
        {
            var typeDef = typeManager.RegisterType(typeName, default, typeof(NativeApiComponent));
            var component = new NativeApiComponent(host, this, typeDef, componentName);
            _components.Add(component);
            return ValueFactory.Create(component);
        }
    }
}
