/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.DebugProtocol.TcpServer;

namespace VSCode.DebugAdapter
{
    public class RpcOperationException : ApplicationException
    {
        public RpcOperationException(RpcExceptionDto exceptionData) : base(exceptionData.Description)
        {
        }
    }
}