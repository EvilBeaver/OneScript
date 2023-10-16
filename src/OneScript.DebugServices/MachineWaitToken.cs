/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Threading;
using ScriptEngine.Machine;

namespace OneScript.DebugServices
{
    public class MachineWaitToken: IDisposable
    {
        private ManualResetEventSlim _threadEvent = new ManualResetEventSlim();

        public MachineInstance Machine { get; set; }

        public void Wait() => _threadEvent.Wait();

        public void Set() => _threadEvent.Set();
        
        public void Reset() => _threadEvent.Reset();
        
        public void Dispose()
        {
            Machine.UnsetDebugMode();
            _threadEvent.Set();
            _threadEvent.Dispose();
        }
    }
}