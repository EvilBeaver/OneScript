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
