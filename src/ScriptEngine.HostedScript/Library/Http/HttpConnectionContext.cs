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
        InternetProxyContext _proxy;
        Uri _hostUri;

        const string HTTP_SCHEME = "http";
        const string HTTPS_SCHEME = "https";

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

            if (uriBuilder.Scheme != HTTP_SCHEME && uriBuilder.Scheme != HTTPS_SCHEME)
                throw RuntimeException.InvalidArgumentValue();

            _hostUri = uriBuilder.Uri;

            Host = _hostUri.Host;
            Port = _hostUri.Port;

            User = user == null ? String.Empty : user;
            Password = password == null ? String.Empty : password;
            Timeout = timeout;
            _proxy = proxy;
            
        }

        [ContextProperty("Пользователь","User")]
        public string User 
        { 
            get; private set;
        }

        [ContextProperty("Пароль", "Password")]
        public string Password
        {
            get; private set;
            
        }

        [ContextProperty("Сервер", "Host")]
        public string Host
        {
            get; private set;
        }

        [ContextProperty("Порт", "Port")]
        public int Port
        {
            get; private set;
        }

        [ContextProperty("Прокси", "Proxy")]
        public IValue Proxy
        {
            get
            {
                if (_proxy == null)
                    return ValueFactory.Create();

                return _proxy;
            }
        }

        [ContextProperty("Таймаут", "Timeout")]
        public int Timeout
        {
            get; private set;
        }

        [ContextMethod("Получить", "Get")]
        public HttpResponseContext Get(HttpRequestContext request, string output = null)
        {
            var webRequest = CreateRequest(request.ResourceAddress);
            webRequest.Method = "GET";

            var response = (HttpWebResponse)webRequest.GetResponse();
            var responseContext = new HttpResponseContext(response);
            if (output != null)
                responseContext.WriteOut(output);

            return responseContext;
        }

        [ContextMethod("Записать", "Put")]
        public HttpResponseContext Put(HttpRequestContext request)
        {
            throw new NotImplementedException();
        }

        private HttpWebRequest CreateRequest(string resource)
        {
            var uriBuilder = new UriBuilder(_hostUri);
            if(Port != 0)
                uriBuilder.Port = Port;
            uriBuilder.Scheme = "http";

            var resourceUri = new Uri(uriBuilder.Uri, resource);

            var request = (HttpWebRequest)HttpWebRequest.Create(resourceUri);
            if(User != "" || Password != "")
                request.Credentials = new NetworkCredential(User, Password);

            request.Proxy = _proxy.GetProxy();
            if (Timeout > 0)
                request.Timeout = Timeout;

            return request;
            
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
                ContextValuesMarshaller.ConvertParam<string>(user),
                ContextValuesMarshaller.ConvertParam<string>(password),
                ContextValuesMarshaller.ConvertParam<InternetProxyContext>(proxy),
                ContextValuesMarshaller.ConvertParam<int>(timeout)
                );
        }

    }
}
