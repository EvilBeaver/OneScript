/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript.Library
{
    [GlobalContext(Category = "Работа с настройками системы")]
    public class SystemConfigAccessor : GlobalContextBase<SystemConfigAccessor>
    {
        private KeyValueConfig _config;

        internal EngineConfigProvider Provider { get; set; }

        public SystemConfigAccessor()
        {
            Refresh();
        }

        internal KeyValueConfig GetConfig()
        {
            return _config;
        }

        /// <summary>
        /// Метод обновляет текущие настройки значениями из файла oscript.cfg
        /// </summary>
        [ContextMethod("ОбновитьНастройкиСистемы", "RefreshSystemConfig")]
        public void Refresh()
        {
            if (Provider != null)
                _config = Provider.ReadConfig();
        }

        /// <summary>
        /// Метод возвращает значение из файла oscript.cfg по имени настойки
        /// </summary>
        /// <param name="optionKey">Имя настройки из файла oscript.cfg</param>
        /// <returns>Строка. Значение системной настройки.</returns>
        [ContextMethod("ПолучитьЗначениеСистемнойНастройки", "GetSystemOptionValue")]
        public IValue GetSystemOptionValue(string optionKey)
        {
            string value = null;
            if (_config != null)
            {
                value = _config[optionKey];
            }

            if (value != null)
                return ValueFactory.Create(value);
            
            return ValueFactory.Create();
        }

        public static IAttachableContext CreateInstance()
        {
            return new SystemConfigAccessor();
        }
    }
}
