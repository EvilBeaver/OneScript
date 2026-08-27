/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

#nullable enable

namespace OneScript.Execution
{
    /// <summary>
    /// Bsl-обёртка потока исполнения процесса.
    ///
    /// Процесс носит обёртку с собой и завершает её, когда освобождается сам. Что за обёртка
    /// и что она хранит, процессу знать не нужно.
    /// </summary>
    public interface IBslExecutionThread
    {
        /// <summary>
        /// Завершает поток исполнения.
        /// </summary>
        void Terminate();
    }
}
