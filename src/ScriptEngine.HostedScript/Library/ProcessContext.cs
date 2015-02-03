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

        public ProcessContext(System.Diagnostics.Process p)
        {
            this._p = p;
        }

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

        [ContextMethod("Запустить", "Start")]
        public void Start()
        {
            _p.Start();
        }

        [ContextProperty("Завершен","HasExited")]
        public bool HasExited
        {
            get
            {
                return _p.HasExited;
            }
        }

        [ContextProperty("КодВозврата", "ExitCode")]
        public int ExitCode
        {
            get
            {
                return _p.ExitCode;
            }
        }

        [ContextMethod("ОжидатьЗавершения", "WaitForExit")]
        public void WaitForExit()
        {
            _p.WaitForExit();
        }

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
    }
}
