/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using Newtonsoft.Json;

namespace OneScript.DebugProtocol.TcpServer
{
    [Serializable]
    [JsonConverter(typeof(JsonDtoConverter))]
    public class RpcCall : TcpProtocolDtoBase
    {
        public object[] Parameters { get; set; }

        public static RpcCall Create(string serviceName, string method, params object[] parameters)
        {
            return new RpcCall
            {
                Id = method,
                ServiceName = serviceName,
                Parameters = parameters
            };
        }
    }
}