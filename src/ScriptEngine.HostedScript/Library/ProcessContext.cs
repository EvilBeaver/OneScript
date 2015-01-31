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
        private System.Diagnostics.Process p;
        private StdTextReadStream _stdOutContext;
        private StdTextReadStream _stdErrContext;

        public ProcessContext(System.Diagnostics.Process p)
        {
            this.p = p;
        }

        [ContextProperty("ПотокВывода", "StdOut")]
        public StdTextReadStream StdOut
        {
            get
            {
                if(_stdOutContext == null)
                    _stdOutContext = new StdTextReadStream(p.StandardOutput);
                return _stdOutContext;
            }
        }

        [ContextProperty("ПотокОшибок", "StdErr")]
        public StdTextReadStream StdErr
        {
            get
            {
                if (_stdErrContext == null)
                    _stdErrContext = new StdTextReadStream(p.StandardError);
                return _stdErrContext;
            }
        }

        [ContextMethod("Запустить", "Start")]
        public void Start()
        {
            p.Start();
        }

        [ContextProperty("Завершен","HasExited")]
        public bool HasExited
        {
            get
            {
                return p.HasExited;
            }
        }

        [ContextProperty("КодВозврата", "ExitCode")]
        public int ExitCode
        {
            get
            {
                return p.ExitCode;
            }
        }

        [ContextMethod("ОжидатьЗавершения", "WaitForExit")]
        public void WaitForExit()
        {
            p.WaitForExit();
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

            p.Dispose();
        }
    }
}
