/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Globalization;
using ScriptEngine;
using ScriptEngine.Hosting;

namespace OneScript.StandardLibrary.Tasks
{
    /// <summary>
    /// Настройки менеджеров фоновых заданий из конфигурации
    /// </summary>
    public class BackgroundTasksOptions
    {
        public const string CAPACITY_KEY_NAME = "backgroundJobs.capacity";

        public BackgroundTasksOptions(KeyValueConfig config)
        {
            Capacity = ResolveCapacity(config[CAPACITY_KEY_NAME]);
        }

        /// <summary>
        /// Сколько заданий хранит ФоновыеЗадания
        /// </summary>
        public int Capacity { get; }

        private static int ResolveCapacity(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return BackgroundTasksManager.DefaultCapacity;

            if (!int.TryParse(rawValue.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var capacity)
                || capacity == 0)
            {
                SystemLogger.Write($"Invalid value for {CAPACITY_KEY_NAME}: {rawValue}");
                return BackgroundTasksManager.DefaultCapacity;
            }

            return capacity;
        }
    }
}
