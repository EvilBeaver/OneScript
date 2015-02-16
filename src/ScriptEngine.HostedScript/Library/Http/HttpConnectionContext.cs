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
        HttpWebRequest _webRequest;
        NetworkCredential _creds;
        InternetProxyContext _proxy;

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

            _webRequest = (HttpWebRequest)WebRequest.Create(uriBuilder.Uri);
            
            if(user != null || password != null)
            {
                _creds = new NetworkCredential(user, password);
                _webRequest.Credentials = _creds;
            }
            else
            {
                _webRequest.Credentials = CredentialCache.DefaultCredentials;
            }

            if (proxy != null)
            {
                _webRequest.Proxy = proxy.GetProxy();
                _proxy = proxy;
            }
            _webRequest.Timeout = timeout;
            
        }

        [ContextProperty("Пользователь","User")]
        public string User 
        { 
            get
            {
                if (_creds == null)
                    return "";

                return _creds.UserName;
            }
        }

        [ContextProperty("Пароль", "Password")]
        public string Password
        {
            get
            {
                if (_creds == null)
                    return "";

                return _creds.Password;
            }
        }

        [ContextProperty("Сервер", "Host")]
        public string Host
        {
            get
            {
                return _webRequest.RequestUri.Host;
            }
        }

        [ContextProperty("Порт", "Port")]
        public int Port
        {
            get
            {
                return _webRequest.RequestUri.Port;
            }
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
            get
            {
                return _webRequest.Timeout;
            }
        }

        [ContextMethod("Получить", "Get")]
        public HttpResponseContext Get(HttpRequestContext request, string output = null)
        {
            var response = (HttpWebResponse)_webRequest.GetResponse();
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
