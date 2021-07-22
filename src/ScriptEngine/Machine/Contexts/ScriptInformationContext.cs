/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language;
using OneScript.Types;
using OneScript.Sources;

namespace ScriptEngine.Machine.Contexts
{
    /// <summary>
    /// Информация о выполняемом сценарии.
    /// </summary>
    [ContextClass("ИнформацияОСценарии", "ScriptInformation")]
    public class ScriptInformationContext : AutoContext<ScriptInformationContext>
    {
        private readonly string _origin;

        internal ScriptInformationContext(ModuleInformation info)
        {
            _origin = info.Origin;
        }

        public ScriptInformationContext(SourceCode codeSrc)
        {
            _origin = codeSrc.Location;
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

        [ContextProperty("Каталог", "Path")]
        public string Path
        {
            get
            {
                if (System.IO.File.Exists(_origin) || System.IO.Directory.Exists(_origin))
                {
                    return System.IO.Path.GetDirectoryName(_origin);
                }
                else
                {
                    return System.IO.Directory.GetCurrentDirectory();
                }
            }
        }

    }
}
