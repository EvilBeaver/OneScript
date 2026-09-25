/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Threading;

namespace ScriptEngine.Machine.Contexts
{
    /// <summary>
    /// Блокировки ресурсов, которые захватывал bsl-процесс.
    /// Монитор привязан к потоку: если процесс завершился, не отпустив блокировку
    /// (например, фоновое задание отменили), освободить ее из другого потока уже нельзя.
    /// Процесс отпускает такие блокировки при завершении, в своем же потоке.
    /// Сколько раз блокировка захвачена и отпущена ли она, знает сам монитор,
    /// поэтому здесь хранятся только объекты блокировок.
    /// </summary>
    internal class ProcessResourceLocks
    {
        private readonly List<object> _entries = new List<object>();

        public void Entered(object lockObject)
        {
            // Уже отпущенные блокировки не нужны, иначе список рос бы на каждой новой блокировке
            _entries.RemoveAll(entry => !Monitor.IsEntered(entry));

            foreach (var entry in _entries)
            {
                if (ReferenceEquals(entry, lockObject))
                    return;
            }

            _entries.Add(lockObject);
        }

        public void ReleaseAll()
        {
            foreach (var lockObject in _entries)
            {
                while (Monitor.IsEntered(lockObject))
                    Monitor.Exit(lockObject);
            }

            _entries.Clear();
        }
    }
}
