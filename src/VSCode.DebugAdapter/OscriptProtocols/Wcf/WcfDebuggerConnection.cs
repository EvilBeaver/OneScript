/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.ServiceModel;
using OneScript.DebugProtocol;

namespace VSCode.DebugAdapter
{
    public class WcfDebuggerConnection : IDebuggerService
    {
        private readonly int _port;
        private readonly IDebugEventListener _eventBackChannel;
        private ServiceProxy<IDebuggerService> _serviceProxy;

        public WcfDebuggerConnection(int port, IDebugEventListener eventBackChannel)
        {
            _port = port;
            _eventBackChannel = eventBackChannel;
        }

        public void Connect()
        {
            var binding = (NetTcpBinding) Binder.GetBinding();
            binding.MaxBufferPoolSize = DebuggerSettings.MAX_BUFFER_SIZE;
            binding.MaxBufferSize = DebuggerSettings.MAX_BUFFER_SIZE;
            binding.MaxReceivedMessageSize = DebuggerSettings.MAX_BUFFER_SIZE;

            var channelFactory = new DuplexChannelFactory<IDebuggerService>(_eventBackChannel, binding,
                new EndpointAddress(Binder.GetDebuggerUri(_port)));
            _serviceProxy = new ServiceProxy<IDebuggerService>(channelFactory.CreateChannel);
        }

        public void Execute(int threadId)
        {
            _serviceProxy.Instance.Execute(threadId);
        }

        public Breakpoint[] SetMachineBreakpoints(Breakpoint[] breaksToSet)
        {
            return _serviceProxy.Instance.SetMachineBreakpoints(breaksToSet);
        }

        public StackFrame[] GetStackFrames(int threadId)
        {
            return _serviceProxy.Instance.GetStackFrames(threadId);
        }

        public Variable[] GetVariables(int threadId, int frameIndex, int[] path)
        {
            return _serviceProxy.Instance.GetVariables(threadId, frameIndex, path);
        }

        public Variable[] GetEvaluatedVariables(string expression, int threadId, int frameIndex, int[] path)
        {
            return _serviceProxy.Instance.GetEvaluatedVariables(expression, threadId, frameIndex, path);
        }

        public Variable Evaluate(int threadId, int contextFrame, string expression)
        {
            return _serviceProxy.Instance.Evaluate(threadId, contextFrame, expression);
        }

        public void Next(int threadId)
        {
            _serviceProxy.Instance.Next(threadId);
        }

        public void StepIn(int threadId)
        {
            _serviceProxy.Instance.StepIn(threadId);
        }

        public void StepOut(int threadId)
        {
            _serviceProxy.Instance.StepOut(threadId);
        }

        public int[] GetThreads()
        {
            return _serviceProxy.Instance.GetThreads();
        }
    }
}