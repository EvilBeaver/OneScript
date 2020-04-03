/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace OneScript.DebugProtocol
{
    [Serializable]
    public class TcpProtocolDto
    {
        public string Id { get; set; }
        
        public object[] Parameters { get; set; }
        
        public static TcpProtocolDto Create(string name, params object[] data)
        {
            return new TcpProtocolDto()
            {
                Id = name,
                Parameters = data
            };
        }
    }
}