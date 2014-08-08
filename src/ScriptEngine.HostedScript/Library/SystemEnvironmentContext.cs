using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Library
{
    [ContextClass("СистемнаяИнформация", "SystemInfo")]
    public class SystemEnvironmentContext : AutoContext<SystemEnvironmentContext>
    {
        [ContextProperty("ИмяКомпьютера", "MachineName")]
        public string MachineName 
        {
            get
            {
                return System.Environment.MachineName;
            }
        }

        [ContextProperty("ВерсияОС", "OSVersion")]
        public string OSVersion
        {
            get
            {
                return System.Environment.OSVersion.VersionString;
            }
        }

        [ContextMethod("ПеременныеСреды", "EnvironmentVariables")]
        public IRuntimeContextInstance EnvironmentVariables()
        {
            var varsMap = new MapImpl();
            var allVars = System.Environment.GetEnvironmentVariables();
            foreach (DictionaryEntry item in allVars)
            {
                varsMap.Insert(
                    ValueFactory.Create((string)item.Key),
                    ValueFactory.Create((string)item.Value));
            }

            return varsMap;
        }
        
        [ScriptConstructor]
        public static IRuntimeContextInstance Create()
        {
            return new SystemEnvironmentContext();
        }
    }
}
