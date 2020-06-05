/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace OneScript.DebugProtocol.Abstractions
{
    public class ChannelException : ApplicationException
    {
        public ChannelException(string message, Exception innerException = null) : base(message, innerException)
        {
        }
        
        public ChannelException(string message, bool isCritical, Exception innerException = null) : base(message, innerException)
        {
            StopChannel = isCritical;
        }
        
        public bool StopChannel { get; set; }
    }
}