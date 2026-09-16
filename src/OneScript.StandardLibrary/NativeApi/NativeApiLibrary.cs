/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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

        private readonly string _identifier;
        private readonly String _tempfile;
        private string[] _classNames;
        private readonly Dictionary<string, string> _extensionToClassName =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _checkedKeys =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> _knownExtensionNames = new List<string>();
        private bool _allKeysEnumerated;

        public NativeApiLibrary(string filepath, string identifier, ITypeManager typeManager)
        {
            _identifier = identifier;

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
            LoadClassNames();
            if (!NativeApiFactory.AllowFactoryClassNames)
                return;

            foreach (String name in _classNames)
                typeManager.RegisterType($"AddIn.{identifier}.{name}", default, typeof(NativeApiFactory));
        }

        private void LoadClassNames()
        {
            var funcPtr = NativeApiKernel.GetProcAddress(Module, "GetClassNames");
            if (funcPtr == IntPtr.Zero) 
                throw new RuntimeException("В библиотеке внешних компонент не обнаружена функция: GetClassNames()");
            var namesPtr = Marshal.GetDelegateForFunctionPointer<GetClassNames>(funcPtr)();
            if (namesPtr == IntPtr.Zero) 
                throw new RuntimeException("Не удалось получить список компонент в составе библиотеки");
            var separator = new char[] { '|' };
            _classNames = NativeApiProxy.Str(namesPtr).Split(separator, StringSplitOptions.RemoveEmptyEntries);
        }

        internal string ResolveClassName(string name)
        {
            if (!NativeApiFactory.AllowFactoryClassNames || _classNames == null)
                return name;

            foreach (var className in _classNames)
            {
                if (string.Equals(className, name, StringComparison.OrdinalIgnoreCase))
                    return className;
            }

            return name;
        }

        public IValue CreateComponent(ITypeManager typeManager, object host, String typeName, String componentName)
        {
            var typeDef = typeManager.GetTypeByName(typeName);

            if (_extensionToClassName.TryGetValue(componentName, out var cachedClassName))
                return TrackComponent(CreateComponentByClassName(host, typeDef, cachedClassName, componentName));

            if (_allKeysEnumerated)
                throw CreateNotFoundException(componentName);

            if (NativeApiFactory.AllowFactoryClassNames)
            {
                var resolvedName = ResolveClassName(componentName);
                var component = TryCreateComponent(host, typeDef, resolvedName);
                if (component != null)
                    return TrackComponent(component);

                component = TryCreateComponent(host, typeDef, componentName);
                if (component != null)
                    return TrackComponent(component);
            }

            NativeApiComponent matched = null;
            foreach (var className in _classNames)
            {
                if (_checkedKeys.Contains(className))
                    continue;

                _checkedKeys.Add(className);

                var candidate = TryCreateComponent(host, typeDef, className);
                if (candidate == null)
                    continue;

                var extensionName = candidate.GetExtensionName();
                if (!string.IsNullOrEmpty(extensionName))
                {
                    _extensionToClassName[extensionName] = className;
                    if (!_knownExtensionNames.Any(n => string.Equals(n, extensionName, StringComparison.OrdinalIgnoreCase)))
                        _knownExtensionNames.Add(extensionName);

                    if (string.Equals(extensionName, componentName, StringComparison.OrdinalIgnoreCase))
                    {
                        matched = candidate;
                        break;
                    }
                }

                candidate.Dispose();
            }

            if (_classNames != null && _checkedKeys.Count >= _classNames.Length)
                _allKeysEnumerated = true;

            if (matched != null)
                return TrackComponent(matched);

            throw CreateNotFoundException(componentName);
        }

        private NativeApiComponent CreateComponentByClassName(
            object host,
            TypeDescriptor typeDef,
            string className,
            string componentName)
        {
            var component = TryCreateComponent(host, typeDef, className);
            if (component == null)
                throw new RuntimeException(
                    $"Не удалось создать объект `{componentName}` внешней компоненты `{_identifier}`");
            return component;
        }

        private NativeApiComponent TryCreateComponent(object host, TypeDescriptor typeDef, string className)
        {
            var component = new NativeApiComponent(host, this, typeDef, className, _identifier, throwOnZero: false);
            if (!component.IsCreated)
            {
                component.Dispose();
                return null;
            }

            return component;
        }

        private IValue TrackComponent(NativeApiComponent component)
        {
            _components.Add(component);
            return component;
        }

        private RuntimeException CreateNotFoundException(string componentName)
        {
            var message = new StringBuilder();
            message.Append(
                $"Не удалось создать объект `{componentName}` внешней компоненты `{_identifier}`");

            if (_knownExtensionNames.Count > 0)
            {
                message.Append(". Доступны: ");
                message.Append(string.Join(", ", _knownExtensionNames));
            }

            return new RuntimeException(message.ToString());
        }

        public void Dispose()
        {
            foreach (var component in _components)
            {
                component.Dispose();
            }
            _components.Clear();

            if (Loaded && NativeApiKernel.FreeLibrary(Module))
            {
                if (!String.IsNullOrEmpty(_tempfile))
                {
                    File.Delete(_tempfile);
                }
            }

            Module = IntPtr.Zero;
        }
    }
}
