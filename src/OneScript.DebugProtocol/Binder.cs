/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

namespace OneScript.DebugProtocol
{
    public static class Binder
    {
        public static Uri GetDebuggerUri(int port)
        {
            var builder = new UriBuilder();
            builder.Scheme = "net.tcp";
            builder.Port = port;
            builder.Host = "localhost";

            return builder.Uri;
        }

        public static Binding GetBinding()
        {
            return new NetTcpBinding(SecurityMode.None);
        }
    }
}
