/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Compilation;
using OneScript.Execution;
using OneScript.Sources;
using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript
{
    /// <summary>
    /// Скомпилированный процесс выполнения скрипта.
    /// </summary>
    public class Process
    {
        private readonly ScriptingEngine _engine;
        private readonly IExecutableModule _module;
        private readonly IBslProcess _bslProcess;

        private Process(
            IBslProcess process,
            IExecutableModule src,
            ScriptingEngine runtime)
        {
            _engine = runtime;
            _module = src;
            _bslProcess = process;
        }

        /// <summary>
        /// Создаёт процесс: выделяет runtime-процесс, компилирует исходник, готовит к запуску.
        /// </summary>
        internal static Process Create(
            ScriptingEngine engine,
            ICompilerFrontend compiler,
            SourceCode source)
        {
            var bslProcess = engine.NewProcess();
            var module = compiler.Compile(source, bslProcess);
            return new Process(bslProcess, module, engine);
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
