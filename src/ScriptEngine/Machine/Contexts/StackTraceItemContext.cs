using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptEngine.Machine.Contexts
{
    /// <summary>
    /// Описание места происхождения исключения.
    /// </summary>
    [ContextClass("КадрСтекаВызовов","CallStackFrame")]
    public class StackTraceItemContext : AutoContext<StackTraceItemContext>
    {
        [ContextProperty("Метод", CanWrite = false)]
        public string Method { get; set; }

        [ContextProperty("НомерСтроки", CanWrite = false)]
        public int LineNumber { get; set; }

        [ContextProperty("ИмяМодуля", CanWrite = false)]
        public string ModuleName { get; set; }

        public override string AsString()
        {
            return $"{Method}: {LineNumber} ({ModuleName})";
        }
    }
}
