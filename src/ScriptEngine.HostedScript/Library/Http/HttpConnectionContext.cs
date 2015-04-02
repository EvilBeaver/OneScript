using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ScriptEngine.HostedScript.Library.Http
{
    /// <summary>
    /// Объект доступа к протоколу HTTP/HTTPS.
    /// Использует семантику методов, реализованных в платформе 1С:Предприятие 8.2.18 и старше.
    /// Синтаксис методов, применявшийся в более младших версиях не поддерживается.
    /// Средства работы с HTTP находятся в статусе experimental.
    /// </summary>
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

        /// <summary>
        /// Получить данные методом GET
        /// </summary>
        /// <param name="request">HTTPЗапрос. Данные и заголовки запроса http</param>
        /// <param name="output">Строка. Имя файла, в который нужно записать ответ. Необязательный параметр.</param>
        /// <returns>HTTPОтвет. Ответ сервера.</returns>
        [ContextMethod("Получить", "Get")]
        public HttpResponseContext Get(HttpRequestContext request, string output = null)
        {
            return GetResponse(request, "GET", output);
        }

        /// <summary>
        /// Передача данных методом PUT
        /// </summary>
        /// <param name="request">HTTPЗапрос. Данные и заголовки запроса http</param>
        /// <returns>HTTPОтвет. Ответ сервера.</returns>
        [ContextMethod("Записать", "Put")]
        public HttpResponseContext Put(HttpRequestContext request)
        {
            return GetResponse(request, "PUT");
        }

        /// <summary>
        /// Передача данных методом POST
        /// </summary>
        /// <param name="request">HTTPЗапрос. Данные и заголовки запроса http</param>
        /// <param name="output">Строка. Имя файла, в который нужно записать ответ. Необязательный параметр.</param>
        /// <returns>HTTPОтвет. Ответ сервера.</returns>
        [ContextMethod("ОтправитьДляОбработки", "Post")]
        public HttpResponseContext Post(HttpRequestContext request, string output = null)
        {
            return GetResponse(request, "POST", output);
        }

        /// <summary>
        /// Удалить данные методом DELETE
        /// </summary>
        /// <param name="request">HTTPЗапрос. Данные и заголовки запроса http</param>
        /// <returns>HTTPОтвет. Ответ сервера.</returns>
        [ContextMethod("Удалить", "Delete")]
        public HttpResponseContext Delete(HttpRequestContext request)
        {
            return GetResponse(request, "DELETE");
        }

        private HttpWebRequest CreateRequest(string resource)
        {
            var uriBuilder = new UriBuilder(_hostUri);
            if(Port != 0)
                uriBuilder.Port = Port;
            
            var resourceUri = new Uri(uriBuilder.Uri, resource);

            var request = (HttpWebRequest)HttpWebRequest.Create(resourceUri);
            if (User != "" || Password != "")
            {
                request.Credentials = new NetworkCredential(User, Password);
                // Авторизация на сервере 1С:Предприятие, например, не работает без явного указания заголовка.
                // http://blog.kowalczyk.info/article/at3/Forcing-basic-http-authentication-for-HttpWebReq.html
                string authInfo = User + ":" + Password;
                // Для 1С работает только UTF-8, хотя стандарт требует ISO-8859-1
                var basicAuthEncoding = Encoding.GetEncoding("UTF-8");
                authInfo = Convert.ToBase64String(basicAuthEncoding.GetBytes(authInfo));
                request.Headers["Authorization"] = "Basic " + authInfo;
            }

            if(_proxy != null)
                request.Proxy = _proxy.GetProxy();

            if (Timeout > 0)
                request.Timeout = Timeout;

            return request;
            
        }

        private HttpResponseContext GetResponse(HttpRequestContext request, string method, string output = null)
        {
            var webRequest = CreateRequest(request.ResourceAddress);
            webRequest.Method = method;
            webRequest.KeepAlive = false;
            SetRequestHeaders(request, webRequest);
            SetRequestBody(request, webRequest);

            HttpWebResponse response;

            try
            {
                response = (HttpWebResponse)webRequest.GetResponse();
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                    response = (HttpWebResponse)ex.Response;
                else
                    throw;
            }

            var responseContext = new HttpResponseContext(response, output);
            
            return responseContext;

        }

        private static void SetRequestBody(HttpRequestContext request, HttpWebRequest webRequest)
        {
            var stream = request.Body;
            if (stream == null)
            {
                return; // тело не установлено
            }

            using(stream)
            {
                if (stream.CanSeek)
                    webRequest.ContentLength = stream.Length;

                using(var requestStream = webRequest.GetRequestStream())
                {
                    const int CHUNK_SIZE = 4096;
                    byte[] buf = new byte[CHUNK_SIZE];
                    
                    while(true)
                    {
                        int bytesRead = stream.Read(buf, 0, CHUNK_SIZE);
                        if (bytesRead == 0)
                            break;

                        requestStream.Write(buf, 0, bytesRead);

                    }
                }
            }
        }

        private static void SetRequestHeaders(HttpRequestContext request, HttpWebRequest webRequest)
        {
            foreach (var item in request.Headers.Select(x => x.GetRawValue() as KeyAndValueImpl))
            {
                System.Diagnostics.Trace.Assert(item != null);

                var key = item.Key.AsString();
                var value = item.Value.AsString();

                switch(key.ToUpperInvariant())
                {
                    case "CONTENT-TYPE":
                        webRequest.ContentType = value;
                        break;
                    case "CONTENT-LENGTH":
                        try
                        {
                            webRequest.ContentLength = Int32.Parse(value);
                        }
                        catch (FormatException)
                        {
                            throw new RuntimeException("Заголовок Content-Length задан неправильно");
                        }
                        break;
                    case "ACCEPT":
                        webRequest.Accept = value;
                        break;
                    case "EXPECT":
                        webRequest.Expect = value;
                        break;
                    case "TRANSFER-ENCODING":
                        webRequest.TransferEncoding = value;
                        break;
                    case "CONNECTION":
                        webRequest.Connection = value;
                        break;
                    case "DATE":
                        try 
	                    {	        
		                    webRequest.Date = DateTime.Parse(value);
	                    }
	                    catch (FormatException)
	                    {
		                    throw new RuntimeException("Заголовок Date задан неправильно");
	                    }
                        break;
                    case "HOST":
                        webRequest.Host = value;
                        break;
                    case "IF-MODIFIED-SINCE":
                        try
                        {
                            webRequest.IfModifiedSince = DateTime.Parse(value);
                        }
                        catch (FormatException)
                        {
                            throw new RuntimeException("Заголовок If-Modified-Since задан неправильно");
                        }
                        break;
                    case "RANGE":
                        throw new NotImplementedException();
                    case "REFERER":
                        webRequest.Referer = value;
                        break;
                    case "USER-AGENT":
                        webRequest.UserAgent = value;
                        break;
                    case "PROXY-CONNECTION":
                        throw new NotImplementedException();
                    default:
                        webRequest.Headers.Set(key, value);
                        break;
                           
                }
                
                

            }
        }

        /// <summary>
        /// Стандартный конструктор. Поддержка клиентских сертификатов HTTPS в текущей версии не реализована.
        /// Для доступа к серверу по протоколу HTTPS указывайте схему https:// в URL.
        /// </summary>
        /// <param name="host">Адрес сервера (можно указать URL-схему http или https)</param>
        /// <param name="port">Порт сервера</param>
        /// <param name="user">Пользователь</param>
        /// <param name="password">Пароль</param>
        /// <param name="proxy">ИнтернетПрокси. Настройки прокси-сервера</param>
        /// <param name="timeout">Таймаут ожидания.</param>
        /// <returns></returns>
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
