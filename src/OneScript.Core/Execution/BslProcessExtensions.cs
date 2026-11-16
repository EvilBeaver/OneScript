/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace OneScript.Execution
{
    public static class BslProcessExtensions
    {
        /// <summary>
        /// Исключение прерывает процесс по запросу отмены.
        /// Такое исключение не должно перехватываться bsl-кодом и превращаться в ScriptException.
        /// </summary>
        public static bool IsCancellationOf(this Exception exception, IBslProcess process)
        {
            return exception is OperationCanceledException canceled
                   && canceled.CancellationToken.IsCancellationRequested
                   && canceled.CancellationToken == process.CancellationToken;
        }
    }
}
