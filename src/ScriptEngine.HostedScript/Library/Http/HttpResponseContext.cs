using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ScriptEngine.HostedScript.Library.Http
{
    [ContextClass("HTTPОтвет", "HTTPResponse")]
    public class HttpResponseContext : AutoContext<HttpResponseContext>
    {
        private MapImpl _headers = new MapImpl();
        private byte[] _body;

        public HttpResponseContext(HttpWebResponse response)
        {
            RetrieveResponseData(response);
        }

        private void RetrieveResponseData(HttpWebResponse response)
        {
            using(response)
            {
                StatusCode = (int)response.StatusCode;
                ProcessHeaders(response.Headers);
                ProcessResponseBody(response);
            }
        }

        private void ProcessHeaders(WebHeaderCollection webHeaderCollection)
        {
            foreach (var item in webHeaderCollection.AllKeys)
            {
                _headers.Insert(ValueFactory.Create(item), ValueFactory.Create(webHeaderCollection[item]));
            }
        }

        private void ProcessResponseBody(HttpWebResponse response)
        {
            if (response.ContentLength == 0)
            {
                _body = new byte[0];
                return;
            }

            _body = new byte[response.ContentLength];
            using(var stream = response.GetResponseStream())
            {
                stream.Read(_body, 0, _body.Length);
            }
        }

        [ContextProperty("Заголовки", "Headers")]
        public MapImpl Headers
        {
            get
            {
                return _headers;
            }
        }

        [ContextProperty("КодСостояния", "StatusCode", CanWrite = false)]
        public int StatusCode { get; set; }

        [ContextMethod(" ", "Alias")]
        public void Alias()
        {

        }
    }
}
