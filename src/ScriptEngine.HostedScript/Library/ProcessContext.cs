/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.Diagnostics;

namespace ScriptEngine.HostedScript.Library
{
    /// <summary>
    /// Позволяет управлять процессом операционной системы. Получать текст из стандартных потоков,
    /// проверять активность, pid, завершать процесс и т.п.
    /// </summary>
    [ContextClass("Процесс", "Process")]
    public class ProcessContext : AutoContext<ProcessContext>, IDisposable
    {
        private System.Diagnostics.Process _p;
        private StdTextReadStream _stdOutContext;
        private StdTextReadStream _stdErrContext;
        private StdTextWriteStream _stdInContext;

        private IValue _outputEncoding;

        private ProcessContext(System.Diagnostics.Process p, IValue encoding)
        {
            _p = p;
            _outputEncoding = encoding;
        }

        public ProcessContext(System.Diagnostics.Process p):this(p, ValueFactory.Create())
        {
        }

        /// <summary>
        /// Устанавливает кодировку в которой будут считываться стандартные потоки вывода и ошибок.
        /// </summary>
        [ContextProperty("КодировкаВывода", "OutputEncoding")]
        public IValue OutputEncoding
        {
            get
            {
                return _outputEncoding;
            }
        }

        /// <summary>
        /// ПотокВыводаТекста. Стандартный поток вывода (stdout)
        /// </summary>
        [ContextProperty("ПотокВывода", "StdOut")]
        public StdTextReadStream StdOut
        {
            get
            {
                if(_stdOutContext == null)
                    _stdOutContext = new StdTextReadStream(_p.StandardOutput);
                    
                return _stdOutContext;
            }
        }

        /// <summary>
        /// ПотокВыводаТекста. Стандартный поток вывода ошибок (stderr)
        /// </summary>
        [ContextProperty("ПотокОшибок", "StdErr")]
        public StdTextReadStream StdErr
        {
            get
            {
                if (_stdErrContext == null)
                    _stdErrContext = new StdTextReadStream(_p.StandardError);
                return _stdErrContext;
            }
        }

        /// <summary>
        /// ПотокВводаТекста. Стандартный поток ввода (stdin)
        /// </summary>
        [ContextProperty("ПотокВвода", "StdIn")]
        public StdTextWriteStream StdIn
        {
            get
            {
                if (_stdInContext == null)
                    _stdInContext = new StdTextWriteStream(_p.StandardInput);
                return _stdInContext;
            }
        }

        /// <summary>
        /// Запустить процесс на выполнение.
        /// </summary>
        [ContextMethod("Запустить", "Start")]
        public void Start()
        {
            _p.Start();
        }

        /// <summary>
        /// Флаг указывает, что процесс завершен (или нет)
        /// </summary>
        [ContextProperty("Завершен","HasExited")]
        public bool HasExited
        {
            get
            {
                return _p.HasExited;
            }
        }

        /// <summary>
        /// Код возврата завершенного процесса.
        /// </summary>
        [ContextProperty("КодВозврата", "ExitCode")]
        public int ExitCode
        {
            get
            {
                return _p.ExitCode;
            }
        }

        /// <summary>
        /// Приостановить выполнение скрипта и ожидать завершения процесса.
        /// </summary>
        [ContextMethod("ОжидатьЗавершения", "WaitForExit")]
        public void WaitForExit()
        {
            _p.WaitForExit();
        }

        /// <summary>
        /// PID процесса
        /// </summary>
        [ContextProperty("Идентификатор", "ProcessId")]
        public int ProcessId
        {
            get
            {
                return _p.Id;
            }
        }

        [ContextMethod("Завершить","Stop")]
        public void Stop()
        {
            _p.Kill();
        }

        public void Dispose()
        {
            if(_stdOutContext != null)
            {
                _stdOutContext.Dispose();
                _stdOutContext = null;
            }

            if (_stdErrContext != null)
            {
                _stdErrContext.Dispose();
                _stdErrContext = null;
            }

            if(_stdInContext != null)
            {
                _stdInContext.Dispose();
                _stdInContext = null;
            }

            _p.Dispose();
        }

        public static ProcessContext Create(string cmdLine, string currentDir = null, bool redirectOutput = false, bool redirectInput = false, IValue encoding = null)
        {
            var sInfo = PrepareProcessStartupInfo(cmdLine, currentDir);
            sInfo.UseShellExecute = false;
            if (redirectInput)
                sInfo.RedirectStandardInput = true;

            if (redirectOutput)
            {
                sInfo.RedirectStandardOutput = true;
                sInfo.RedirectStandardError = true;
            }

            if (encoding != null)
            {
                var enc = TextEncodingEnum.GetEncoding(encoding);

                sInfo.StandardOutputEncoding = enc;
                sInfo.StandardErrorEncoding = enc;
            }

            var p = new System.Diagnostics.Process();
            p.StartInfo = sInfo;

            return new ProcessContext(p, encoding);
        }

        public static System.Diagnostics.ProcessStartInfo PrepareProcessStartupInfo(string cmdLine, string currentDir)
        {
            var sInfo = new System.Diagnostics.ProcessStartInfo();

            var enumArgs = Utils.SplitCommandLine(cmdLine);

            bool fNameRead = false;
            StringBuilder argsBuilder = new StringBuilder();

            foreach (var item in enumArgs)
            {
                if (!fNameRead)
                {
                    sInfo.FileName = item;
                    fNameRead = true;
                }
                else
                {
                    argsBuilder.Append(' ');
                    argsBuilder.Append(item);
                }
            }

            if (argsBuilder.Length > 0)
            {
                argsBuilder.Remove(0, 1);
            }

            sInfo.Arguments = argsBuilder.ToString();
            if (currentDir != null)
                sInfo.WorkingDirectory = currentDir;
            return sInfo;
        }

    }
}
