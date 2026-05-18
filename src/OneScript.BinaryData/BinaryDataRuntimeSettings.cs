/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.DependencyInjection;

namespace OneScript.BinaryData
{
    /// <summary>
    /// Лимит памяти для двоичных данных до перехода на временный файл.
    /// Задаётся один раз при создании движка из конфигурации (см. регистрацию <see cref="IBinaryDataMemoryLimit"/> в стандартной библиотеке).
    /// </summary>
    public static class BinaryDataRuntimeSettings
    {
        private static volatile int _inMemoryMaxBytes = BinaryDataConfigurationDefaults.InMemoryMaxBytes;

        /// <summary>
        /// Читает лимит из DI и сохраняет для использования кодом без доступа к контейнеру (конструктор «Файловый поток с буфером» и т.п.).
        /// Вызывается при создании экземпляра основного движка скриптов.
        /// </summary>
        public static void ApplyFromServices(IServiceContainer services)
        {
            var opts = services.TryResolve<IBinaryDataMemoryLimit>();
            var maxBytes = opts?.MaxBytesInMemory ?? BinaryDataConfigurationDefaults.InMemoryMaxBytes;
            if (maxBytes <= 0 || maxBytes == int.MaxValue)
                maxBytes = BinaryDataConfigurationDefaults.InMemoryMaxBytes;

            _inMemoryMaxBytes = maxBytes;
        }

        /// <summary>
        /// Текущий лимит «в памяти до временного файла» для двоичных данных (байты).
        /// До вызова <see cref="ApplyFromServices"/> равен <see cref="BinaryDataConfigurationDefaults.InMemoryMaxBytes"/>.
        /// </summary>
        public static int GetEffectiveInMemoryMaxBytes() => _inMemoryMaxBytes;
    }
}
