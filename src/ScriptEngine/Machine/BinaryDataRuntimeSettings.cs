/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Threading;
using OneScript.DependencyInjection;

namespace ScriptEngine.Machine
{
    /// <summary>
    /// Лимит памяти для двоичных данных на время выполнения процесса BSL (AsyncLocal).
    /// </summary>
    public static class BinaryDataRuntimeSettings
    {
        private static readonly AsyncLocal<int?> InMemoryMaxBytes = new AsyncLocal<int?>();

        internal static void PushFromServices(IServiceContainer services)
        {
            var opts = services.TryResolve<IBinaryDataMemoryLimit>();
            var maxBytes = opts?.MaxBytesInMemory ?? BinaryDataConfigurationDefaults.InMemoryMaxBytes;
            if (maxBytes <= 0)
                maxBytes = BinaryDataConfigurationDefaults.InMemoryMaxBytes;
            InMemoryMaxBytes.Value = maxBytes;
        }

        internal static void Pop()
        {
            InMemoryMaxBytes.Value = null;
        }

        /// <summary>
        /// Текущий лимит «в памяти до временного файла» для двоичных данных (байты).
        /// Вне процесса BSL возвращает значение по умолчанию из <see cref="BinaryDataConfigurationDefaults.InMemoryMaxBytes"/>.
        /// </summary>
        public static int GetEffectiveInMemoryMaxBytes()
        {
            return InMemoryMaxBytes.Value ?? BinaryDataConfigurationDefaults.InMemoryMaxBytes;
        }
    }
}
