/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Execution;

namespace ScriptEngine.Libraries
{
    /// <summary>
    /// Информация о добавленном пользовательском скрипте.
    /// Используется для сериализации в ApplicationDump.
    /// </summary>
    public class UserAddedScript
    {
        public string Symbol { get; set; }
        public string FilePath { get;  set; }
        public string FileName { get; set; }
        public IExecutableModule Module  { get; set; }
    }
}
