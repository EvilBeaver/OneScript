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

        private readonly HashSet<NativeApiComponent> _components =
            new HashSet<NativeApiComponent>(ReferenceEqualityComparer.Instance);

        private readonly string _identifier;
        private readonly String _tempfile;
        private string[] _classNames;
        private readonly Dictionary<string, string> _extensionToClassName =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _checkedKeys =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> _knownExtensionNames = new List<string>();
        private bool _allKeysEnumerated;

        // Компоненты одной библиотеки создают из разных фоновых заданий и запросов веб-сервера,
        // а кэш имен и список созданных компонент у библиотеки общие
        private readonly object _lock = new object();
        private bool _disposed;

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

            lock (_lock)
            {
                // Библиотеку выгружают при остановке движка, а фоновые задания еще могут работать
                if (_disposed)
                    throw new RuntimeException($"Библиотека внешних компонент `{_identifier}` уже выгружена");

                return DoCreateComponent(host, typeDef, componentName);
            }
        }

        private IValue DoCreateComponent(object host, TypeDescriptor typeDef, String componentName)
        {
            if (_extensionToClassName.TryGetValue(componentName, out var cachedClassName))
                return TrackComponent(CreateComponentByClassName(host, typeDef, cachedClassName, componentName));

            if (_allKeysEnumerated)
                throw CreateNotFoundException(componentName);

            var component = TryCreateByFactoryClassName(host, typeDef, componentName)
                            ?? FindByExtensionName(host, typeDef, componentName);
            if (component != null)
                return TrackComponent(component);

            throw CreateNotFoundException(componentName);
        }

        private NativeApiComponent TryCreateByFactoryClassName(object host, TypeDescriptor typeDef, string componentName)
        {
            if (!NativeApiFactory.AllowFactoryClassNames)
                return null;

            return TryCreateComponent(host, typeDef, ResolveClassName(componentName))
                   ?? TryCreateComponent(host, typeDef, componentName);
        }

        // Создает компоненты еще не проверенных классов и запоминает их имена расширений
        private NativeApiComponent FindByExtensionName(object host, TypeDescriptor typeDef, string componentName)
        {
            NativeApiComponent matched = null;
            foreach (var className in _classNames)
            {
                if (!_checkedKeys.Add(className))
                    continue;

                var candidate = TryCreateComponent(host, typeDef, className);
                if (candidate == null)
                    continue;

                if (RememberExtensionName(candidate, className, componentName))
                {
                    matched = candidate;
                    break;
                }

                candidate.Dispose();
            }

            if (_classNames != null && _checkedKeys.Count >= _classNames.Length)
                _allKeysEnumerated = true;

            return matched;
        }

        // true - имя расширения компоненты совпало с искомым
        private bool RememberExtensionName(NativeApiComponent candidate, string className, string componentName)
        {
            var extensionName = candidate.GetExtensionName();
            if (string.IsNullOrEmpty(extensionName))
                return false;

            _extensionToClassName[extensionName] = className;
            if (!_knownExtensionNames.Any(n => string.Equals(n, extensionName, StringComparison.OrdinalIgnoreCase)))
                _knownExtensionNames.Add(extensionName);

            return string.Equals(extensionName, componentName, StringComparison.OrdinalIgnoreCase);
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

        /// <summary>
        /// Уничтожает объект компоненты и снимает ее с учета: при выгрузке библиотеки ее уничтожать уже не нужно.
        /// Под блокировкой библиотеки ОсвободитьОбъект из другого потока и выгрузка библиотеки
        /// не уничтожат объект дважды, а выгрузка не дойдет до FreeLibrary раньше, чем объект уничтожен.
        /// </summary>
        internal void DestroyComponent(NativeApiComponent component, ref IntPtr nativeObject)
        {
            lock (_lock)
            {
                if (nativeObject == IntPtr.Zero)
                    return;

                NativeApiProxy.DestroyObject(nativeObject);
                nativeObject = IntPtr.Zero;
                _components.Remove(component);
            }
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
            NativeApiComponent[] components;
            lock (_lock)
            {
                // Под той же блокировкой, что и создание: компонента, созданная после снимка, осталась бы жить
                _disposed = true;
                components = _components.ToArray();
                _components.Clear();
            }

            // Не под перебором списка: освобождаемая компонента сама снимает себя с учета.
            // Если ее уже уничтожают из другого потока, Dispose дождется этого на блокировке
            foreach (var component in components)
            {
                component.Dispose();
            }

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
