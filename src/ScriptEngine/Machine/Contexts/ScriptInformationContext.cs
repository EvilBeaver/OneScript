using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Environment;

namespace ScriptEngine.Machine.Contexts
{
    /// <summary>
    /// Информация о выполняемом сценарии.
    /// </summary>
    [ContextClass("ИнформацияОСценарии", "ScriptInformation")]
    public class ScriptInformationContext : AutoContext<ScriptInformationContext>
    {
        private string _origin;

        internal ScriptInformationContext(ModuleInformation info)
        {
            _origin = info.Origin;
        }

        public ScriptInformationContext(ICodeSource codeSrc)
        {
            _origin = codeSrc.SourceDescription;
        }

        /// <summary>
        /// Путь к файлу сценария, если выполняется сценарий из файла. Для всех прочих сценариев возвращаемое значение определяется хост-приложением.
        /// </summary>
        [ContextProperty("Источник", "Source")]
        public string Source
        {
            get
            {
                return _origin;
            }
        }

    }
}
