/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.DebugProtocol
{
    public interface IDebugEventListener
    {
#if NETFRAMEWORK
        [OperationContract(IsOneWay = true)]
#endif
        void ThreadStopped(int threadId, ThreadStopReason reason);

#if NETFRAMEWORK
        [OperationContract(IsOneWay = true)]
#endif
        void ProcessExited(int exitCode);
    }
}