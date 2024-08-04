/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace ScriptEngine.Machine
{
    public interface IDebugController : IDisposable
    {
        void Init();
        void Wait();
        void NotifyProcessExit(int exitCode);

        void AttachToThread();

        void DetachFromThread();
        
        IBreakpointManager BreakpointManager { get; }
    }

    public interface IBreakpointManager
    {
        void SetExceptionBreakpoints(string[] filters);

        void SetBreakpoints(string module, (int Line, string Condition)[] breakpoints);

        bool StopOnAnyException();

        bool StopOnUncaughtException();
        
        bool FindBreakpoint(string module, int line);

        string GetCondition(string module, int line);

        void Clear();
    }
}
