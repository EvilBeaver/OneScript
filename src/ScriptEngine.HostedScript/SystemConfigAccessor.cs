/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library
{
    [GlobalContext(Category = "Работа с настройками системы")]
    public class SystemConfigAccessor : GlobalContextBase<SystemConfigAccessor>
    {
        private readonly EngineConfiguration _activeConfig;

        public SystemConfigAccessor(EngineConfiguration activeConfig)
        {
            _activeConfig = activeConfig;
        }

        /// <summary>
        /// Метод обновляет текущие настройки значениями из файла oscript.cfg
        /// </summary>
        [ContextMethod("ОбновитьНастройкиСистемы", "RefreshSystemConfig")]
        public void Refresh()
        {
            _activeConfig.Reload();
        }

        /// <summary>
        /// Метод возвращает значение из файла oscript.cfg по имени настойки
        /// </summary>
        /// <param name="optionKey">Имя настройки из файла oscript.cfg</param>
        /// <returns>Строка. Значение системной настройки.</returns>
        [ContextMethod("ПолучитьЗначениеСистемнойНастройки", "GetSystemOptionValue")]
        public IValue GetSystemOptionValue(string optionKey)
        {
            var cfg = _activeConfig.GetConfig();
            
            var value = cfg[optionKey];

            return value != null ? ValueFactory.Create(value) : ValueFactory.Create();
        }

        public static IAttachableContext CreateInstance(EngineConfiguration configHolder)
        {
            return new SystemConfigAccessor(configHolder);
        }
    }
}
