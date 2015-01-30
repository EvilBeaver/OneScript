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
    public class ProcessContext : AutoContext<ProcessContext>
    {
        private System.Diagnostics.Process p;

        public ProcessContext(System.Diagnostics.Process p)
        {
            // TODO: Complete member initialization
            this.p = p;
        }

    }
}
