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
    /// <summary>
    /// Класс, ассоциированный с экземпляром библиотеки внешних компонент 
    /// Native API и осуществляющий непосредственное создание экземпляра компоненты.
    /// </summary>
    class NativeApiLibrary
    {
        private delegate IntPtr GetClassNames();

        private readonly List<NativeApiComponent> _components = new List<NativeApiComponent>();

        private readonly String _tempfile;

        public NativeApiLibrary(String filepath, String identifier)
        {
            if (!File.Exists(filepath))
                return;

            using (var stream = File.OpenRead(filepath))
            {
                if (NativeApiPackage.IsZip(stream))
                {
                    _tempfile = Path.GetTempFileName();
                    NativeApiPackage.Extract(stream, _tempfile);
                    Module = NativeApiKernel.LoadLibrary(_tempfile);
                }
                else 
                    Module = NativeApiKernel.LoadLibrary(filepath);
            }
            if (Loaded) 
                RegisterComponents(identifier);
        }

        public void Dispose()
        {
            foreach (var component in _components)
                component.Dispose();
            if (Loaded && NativeApiKernel.FreeLibrary(Module))
                if (!String.IsNullOrEmpty(_tempfile))
                    try { File.Delete(_tempfile); } catch (Exception) { }
        }

        public IntPtr Module { get; private set; } = IntPtr.Zero;

        public Boolean Loaded
        {
            get => Module != IntPtr.Zero;
        }

        private void RegisterComponents(String identifier)
        {
            var funcPtr = NativeApiKernel.GetProcAddress(Module, "GetClassNames");
            if (funcPtr == IntPtr.Zero) 
                throw new RuntimeException("В библиотеке внешних компонент не обнаружена функция: GetClassNames()");
            var namesPtr = Marshal.GetDelegateForFunctionPointer<GetClassNames>(funcPtr)();
            if (namesPtr == IntPtr.Zero) 
                throw new RuntimeException("Не удалось получить список компонент в составе библиотеки");
            var separator = new char[] { '|' };
            var names = NativeApiProxy.Str(namesPtr).Split(separator, StringSplitOptions.RemoveEmptyEntries);
            foreach (String name in names)
                TypeManager.RegisterType($"AddIn.{identifier}.{name}", typeof(NativeApiFactory));
        }

        public IValue CreateComponent(IHostApplication host, String typeName, String componentName)
        {
            var component = new NativeApiComponent(host, this, typeName, componentName);
            _components.Add(component);
            return ValueFactory.Create(component);
        }
    }
}
