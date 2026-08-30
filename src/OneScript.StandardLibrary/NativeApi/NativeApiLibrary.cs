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
using OneScript.Exceptions;
using OneScript.Types;
using ScriptEngine.Machine;

namespace OneScript.StandardLibrary.NativeApi
{
    /// <summary>
    /// Класс, ассоциированный с экземпляром библиотеки внешних компонент 
    /// Native API и осуществляющий непосредственное создание экземпляра компоненты.
    /// </summary>
    class NativeApiLibrary : IDisposable
    {
        private delegate IntPtr GetClassNames();

        private readonly List<NativeApiComponent> _components = new List<NativeApiComponent>();

        private readonly Dictionary<string, string> _nameToClassName =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private readonly String _tempfile;

        public NativeApiLibrary(string filepath, string identifier, ITypeManager typeManager)
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
                    if (Module == IntPtr.Zero)
                    {
                        File.Delete(_tempfile);
                    }
                }
                else 
                    Module = NativeApiKernel.LoadLibrary(filepath);
            }
            if (Loaded) 
                RegisterComponents(identifier, typeManager);
        }

        public IntPtr Module { get; private set; } = IntPtr.Zero;

        public Boolean Loaded
        {
            get => Module != IntPtr.Zero;
        }

        private void RegisterComponents(string identifier, ITypeManager typeManager)
        {
            var funcPtr = NativeApiKernel.GetProcAddress(Module, "GetClassNames");
            if (funcPtr == IntPtr.Zero) 
                throw new RuntimeException("В библиотеке внешних компонент не обнаружена функция: GetClassNames()");
            var namesPtr = Marshal.GetDelegateForFunctionPointer<GetClassNames>(funcPtr)();
            if (namesPtr == IntPtr.Zero) 
                throw new RuntimeException("Не удалось получить список компонент в составе библиотеки");
            var separator = new char[] { '|' };
            var names = NativeApiProxy.Str(namesPtr).Split(separator, StringSplitOptions.RemoveEmptyEntries);
            foreach (String className in names)
            {
                var ptr = NativeApiProxy.GetClassObject(Module, className, null, null, null);
                if (ptr == IntPtr.Zero)
                {
                    typeManager.RegisterType($"AddIn.{identifier}.{className}", default, typeof(NativeApiFactory));
                    continue;
                }

                var extensionName = string.Empty;
                NativeApiProxy.GetExtensionName(ptr, n => extensionName = NativeApiProxy.Str(n));
                NativeApiProxy.DestroyObject(ptr);

                if (string.IsNullOrEmpty(extensionName))
                {
                    typeManager.RegisterType($"AddIn.{identifier}.{className}", default, typeof(NativeApiFactory));
                    continue;
                }

                _nameToClassName[extensionName] = className;

                if (string.Equals(extensionName, className, StringComparison.OrdinalIgnoreCase))
                    typeManager.RegisterType($"AddIn.{identifier}.{extensionName}", default, typeof(NativeApiFactory));
                else
                    typeManager.RegisterType(
                        $"AddIn.{identifier}.{extensionName}",
                        $"AddIn.{identifier}.{className}",
                        typeof(NativeApiFactory));
            }
        }

        internal string ResolveClassName(string name)
        {
            if (_nameToClassName.TryGetValue(name, out var className))
                return className;
            return name;
        }

        public IValue CreateComponent(ITypeManager typeManager, object host, String typeName, String componentName)
        {
            var typeDef = typeManager.GetTypeByName(typeName);
            var resolvedName = ResolveClassName(componentName);
            var component = new NativeApiComponent(host, this, typeDef, resolvedName);
            _components.Add(component);
            return component;
        }

        private void DisposeManagedResources()
        {
            foreach (var component in _components)
            {
                component.Dispose();
            }

            _components.Clear();
        }

        private void ReleaseUnmanagedResources()
        {
            if (!Loaded)
                return;

            NativeApiKernel.FreeLibrary(Module);
            Module = IntPtr.Zero;

            if (!String.IsNullOrEmpty(_tempfile))
            {
                File.Delete(_tempfile);
            }
        }

        public void Dispose()
        {
            DisposeManagedResources();
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~NativeApiLibrary()
        {
            ReleaseUnmanagedResources();
        }
    }
}
