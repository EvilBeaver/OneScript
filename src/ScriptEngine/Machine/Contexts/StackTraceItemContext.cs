/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

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
