using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ScriptEngine.HostedScript.Library.Http
{
    [ContextClass("HTTPСоединение", "HTTPConnection")]
    public class HttpConnectionContext : AutoContext<HttpConnectionContext>
    {
        WebRequest _webRequest;

        public HttpConnectionContext(string host,
            int port = 0,
            string user = null,
            string password = null,
            InternetProxyContext proxy = null,
            int timeout = 0)
        {
            var uriBuilder = new UriBuilder(host);
            if (port != 0)
                uriBuilder.Port = port;

            _webRequest = WebRequest.Create(uriBuilder.Uri);
            if(user != null || password != null)
            {
                var creds = new NetworkCredential(user, password);
                _webRequest.Credentials = creds;
            }
            else
            {
                _webRequest.Credentials = CredentialCache.DefaultCredentials;
            }

            if (proxy != null)
                _webRequest.Proxy = proxy.GetProxy();

            _webRequest.Timeout = timeout;
            
        }


        
        [ScriptConstructor]
        public static HttpConnectionContext Constructor(IValue host, 
            IValue port = null, 
            IValue user = null, 
            IValue password = null,
            IValue proxy = null,
            IValue timeout = null)
        {
            return new HttpConnectionContext(host.AsString(),
                ContextValuesMarshaller.ConvertParam<int>(port),
                ContextValuesMarshaller.ConvertParam<string>(port),
                ContextValuesMarshaller.ConvertParam<string>(password),
                ContextValuesMarshaller.ConvertParam<InternetProxyContext>(proxy),
                ContextValuesMarshaller.ConvertParam<int>(timeout)
                );
        }

    }
}
