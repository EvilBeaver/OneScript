/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using OneScript.Execution;
using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript
{
    /// <summary>
    /// Скомпилированный процесс выполнения скрипта.
    /// </summary>
    public class Process
    {
        readonly ScriptingEngine _engine;
        readonly IExecutableModule _module;
        private readonly IBslProcess _bslProcess;

        internal Process(
            IBslProcess process,
            IExecutableModule src,
            ScriptingEngine runtime)
        {
            _engine = runtime;
            _module = src;
            _bslProcess = process;
        }

        /// <summary>
        /// Запускает выполнение скрипта.
        /// </summary>
        /// <returns>
        /// <c>0</c> при успешном завершении; код выхода при прерывании скрипта.
        /// </returns>
        /// <remarks>
        /// Прочие исключения пробрасываются вызывающему коду.
        /// </remarks>
        public int Start()
        {
            try
            {
                _engine.NewObject(_module, _bslProcess);
                return 0;
            }
            catch (ScriptInterruptionException e)
            {
                return e.ExitCode;
            }
        }

    }
}
