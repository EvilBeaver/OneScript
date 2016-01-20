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

        [ContextMethod("ОбновитьНастройкиСистемы", "RefreshSystemConfig")]
        public void Refresh()
        {
            if (Provider != null)
                _config = Provider.ReadConfig();
        }

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
