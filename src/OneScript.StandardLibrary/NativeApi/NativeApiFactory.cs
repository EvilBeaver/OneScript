/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Types;
using ScriptEngine.Machine;

namespace OneScript.StandardLibrary.NativeApi
{
    /// <summary>
    /// Фабрика, осуществляющая регистрацию библиотеки внешних 
    /// компонент Native API и создания экземпляров компонент.
    /// </summary>
    class NativeApiFactory
    {
        /// <summary>
        /// Разрешает использовать в качестве имени типа ключ фабрики из GetClassNames
        /// или любую строку, которую принимает GetClassObject, помимо имени из
        /// RegisterExtensionAs. Так работал движок до исправления #1359.
        /// Выключено: по спецификации Native API имя типа задаёт только
        /// RegisterExtensionAs, а GetClassNames возвращает ключи фабрики.
        /// Вернуть true, если обнаружатся компоненты, для которых скрипты
        /// полагаются на прежнее поведение.
        /// </summary>
        internal static readonly bool AllowFactoryClassNames = false;

        public static bool Register(string filepath, string identifier, ITypeManager typeManager)
        {
            if (_libraries.ContainsKey(identifier)) 
                return false;
            var library = new NativeApiLibrary(filepath, identifier, typeManager);
            if (library.Loaded) 
                _libraries.Add(identifier, library);
            return library.Loaded;
        }

        private static readonly Dictionary<string, NativeApiLibrary> _libraries =
            new Dictionary<string, NativeApiLibrary>(StringComparer.OrdinalIgnoreCase);

        internal static bool TryGetLibrary(string identifier, out NativeApiLibrary library)
        {
            return _libraries.TryGetValue(identifier, out library);
        }

        private static bool _shutdown;

        internal static void Shutdown()
        {
            if (_shutdown)
                return;

            _shutdown = true;

            foreach (var item in _libraries)
                item.Value.Dispose();
            _libraries.Clear();
        }

        [ScriptConstructor]
        public static IValue Constructor(TypeActivationContext context)
        {
            var typeName = context.TypeName;
            var names = typeName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (names.Length != 3 || !string.Equals(names[0], "AddIn", StringComparison.OrdinalIgnoreCase))
                throw new RuntimeException($"Имя типа `{typeName}` не имеет формы AddIn.<метка>.<имя>");

            if (!_libraries.TryGetValue(names[1], out NativeApiLibrary library))
                throw new RuntimeException($"Внешняя компонента с меткой `{names[1]}` не подключена");

            return library.CreateComponent(context.TypeManager, default, typeName, names[2]);
        }
    }
}
