/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace ScriptEngine.Machine.Debugger
{
    /// <summary>
    /// Отладчик, который ничего не делает. Заглушка для выключенного отладчика.
    /// </summary>
    public class DisabledDebugger : IDebugger
    {
        public bool IsEnabled => false;
        
        public void Start()
        {
        }

        public IDebugSession GetSession() => new DisabledDebugSession();

        public void NotifyProcessExit(int exitCode)
        {
        }
    }
}