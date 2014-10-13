using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Environment;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine.Library
{
    /// <summary>
    /// Информация о выполняемом сценарии.
    /// </summary>
    [ContextClass("ИнформацияОСценарии", "ScriptInformation")]
    class ScriptInformationContext : AutoContext<ScriptInformationContext>
    {
        private ICodeSource _info;

        public ScriptInformationContext(ICodeSource info)
        {
            _info = info;
        }

        /// <summary>
        /// Путь к файлу сценария, если выполняется сценарий из файла. Для всех прочих сценариев возвращаемое значение определяется хост-приложением.
        /// </summary>
        [ContextProperty("Источник", "Source")]
        public string Source
        {
            get
            {
                return _info.SourceDescription;
            }
        }
    }
}
