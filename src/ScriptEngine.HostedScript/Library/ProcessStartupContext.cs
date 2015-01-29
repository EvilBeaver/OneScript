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
    /// Информация для запуска процесса. Позволяет установить начальные параметры создаваемого процесса ОС,
    /// Такие, как перенаправление вывода, рабочий каталог и т.п.
    /// </summary>
    [ContextClass("ЗапускПроцесса", "ProcessStartup")]
    public class ProcessStartupContext : AutoContext<ProcessStartupContext>
    {
        private ProcessStartInfo _startInfo;

        #region Constructors

        public ProcessStartupContext()
        {
            _startInfo = new ProcessStartInfo();
            SetDefaultPSIProperties();
        }

        public ProcessStartupContext(string filename)
        {
            _startInfo = new ProcessStartInfo(filename);
            SetDefaultPSIProperties();
        }

        private void SetDefaultPSIProperties()
        {
            _startInfo.UseShellExecute = false;
        }

        [ScriptConstructor]
        public static ProcessStartupContext Create()
        {
            return new ProcessStartupContext();
        }

        [ScriptConstructor(Name = "По имени приложения")]
        public static ProcessStartupContext Create(IValue filename)
        {
            return new ProcessStartupContext(filename.AsString());
        } 

        #endregion

        [ContextProperty("ИмяФайла", "FileName")]
        public string FileName 
        {
            get { return _startInfo.FileName; }
            set { _startInfo.FileName = value; }
        }

        [ContextProperty("Аргументы", "Arguments")]
        public string Arguments
        {
            get { return _startInfo.Arguments; }
            set { _startInfo.Arguments = value; }
        }
        
        [ContextProperty("РабочийКаталог", "WorkingDirectory")]
        public string WorkingDirectory
        {
            get { return _startInfo.WorkingDirectory; }
            set { _startInfo.WorkingDirectory= value; }
        }

        [ContextProperty("ПеренаправлятьСтандартныйВывод", "RedirectStandardOutput")]
        public bool RedirectStandardOutput
        {
            get { return _startInfo.RedirectStandardOutput; }
            set { _startInfo.RedirectStandardOutput = value; }
        }

        [ContextProperty("ПеренаправлятьСтандартныйВвод", "RedirectStandardInput")]
        public bool RedirectStandardInput
        {
            get { return _startInfo.RedirectStandardInput; }
            set { _startInfo.RedirectStandardInput = value; }
        }

        [ContextProperty("ПеренаправлятьСтандартныйПотокОшибок", "RedirectStandardError")]
        public bool RedirectStandardError
        {
            get { return _startInfo.RedirectStandardError; }
            set { _startInfo.RedirectStandardError = value; }
        }

    }
}
