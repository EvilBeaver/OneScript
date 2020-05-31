/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.ServiceModel;
using OneScript.DebugProtocol;
using StackFrame = OneScript.DebugProtocol.StackFrame;
using Variable = OneScript.DebugProtocol.Variable;
using MachineVariable = ScriptEngine.Machine.Variable;

namespace oscript.DebugServer
{
    [Obsolete]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
    internal class WcfDebugService : IDebuggerService
    {
        private readonly DebugServiceImpl _debugServiceImpl;
        private WcfDebugController Controller { get; }
        
        public WcfDebugService(WcfDebugController controller)
        {
            Controller = controller;
            _debugServiceImpl = new DebugServiceImpl(controller);
        }

        #region WCF Communication methods

        public void Execute(int threadId)
        {
            RegisterEventListener();
            _debugServiceImpl.Execute(threadId);
        }

        private void RegisterEventListener()
        {
            var eventChannel = OperationContext.Current.
                   GetCallbackChannel<IDebugEventListener>();

            Controller.SetCallback(eventChannel);
        }

        public Breakpoint[] SetMachineBreakpoints(Breakpoint[] breaksToSet)
        {
            return _debugServiceImpl.SetMachineBreakpoints(breaksToSet);
        }

        public StackFrame[] GetStackFrames(int threadId)
        {
            return _debugServiceImpl.GetStackFrames(threadId);
        }

        public Variable[] GetVariables(int threadId, int frameId, int[] path)
        {
            return _debugServiceImpl.GetVariables(threadId, frameId, path);
        }

        public Variable[] GetEvaluatedVariables(string expression, int threadId, int frameIndex, int[] path)
        {
            return _debugServiceImpl.GetEvaluatedVariables(expression, threadId, frameIndex, path);
        }

        public Variable Evaluate(int threadId, int contextFrame, string expression)
        {
            return _debugServiceImpl.Evaluate(threadId, contextFrame, expression);
        }

        public void Next(int threadId)
        {
            _debugServiceImpl.Next(threadId);
        }

        public void StepIn(int threadId)
        {
            _debugServiceImpl.StepIn(threadId);
        }

        public void StepOut(int threadId)
        {
            _debugServiceImpl.StepOut(threadId);
        }

        public int[] GetThreads()
        {
            return _debugServiceImpl.GetThreads();
        }

        public int GetProcessId()
        {
            return System.Diagnostics.Process.GetCurrentProcess().Id;
        }

        #endregion
    }
}
