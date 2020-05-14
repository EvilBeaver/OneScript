/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace OneScript.DebugProtocol.TcpServer
{
    [Serializable]
    public class RpcCallResult : TcpProtocolDtoBase
    {
        public object ReturnValue { get; set; }

        public static RpcCallResult Respond(RpcCall call, object value)
        {
            return new RpcCallResult
            {
                Id = call.Id,
                ServiceName = call.ServiceName,
                ReturnValue = value
            };
        }
        
        public static RpcCallResult Exception(RpcCall call, Exception value)
        {
            return new RpcCallResult
            {
                Id = call.Id,
                ServiceName = call.ServiceName,
                ReturnValue = new RpcExceptionDto
                {
                    Description = value.Message
                }
            };
        }
    }
}