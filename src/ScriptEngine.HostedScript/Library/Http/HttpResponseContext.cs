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
    public class HttpResponseContext : AutoContext<HttpResponseContext>, IDisposable
    {
        private MapImpl _headers = new MapImpl();
        // TODO: Нельзя выделить массив размером больше чем 2GB
        // поэтому функционал сохранения в файл не должен использовать промежуточный буфер _body
        private HttpResponseBody _body;
        
        private string _defaultCharset;
        private string _filename;

        public HttpResponseContext(HttpWebResponse response)
        {
            RetrieveResponseData(response, null);
        }

        public HttpResponseContext(HttpWebResponse response, string dumpToFile)
        {
            RetrieveResponseData(response, dumpToFile);
        }

        private void RetrieveResponseData(HttpWebResponse response, string dumpToFile)
        {
            using(response)
            {
                StatusCode = (int)response.StatusCode;
                _defaultCharset = response.CharacterSet;

                ProcessHeaders(response.Headers);
                ProcessResponseBody(response, dumpToFile);
            }
        }

        private void ProcessHeaders(WebHeaderCollection webHeaderCollection)
        {
            foreach (var item in webHeaderCollection.AllKeys)
            {
                _headers.Insert(ValueFactory.Create(item), ValueFactory.Create(webHeaderCollection[item]));
            }
        }

        private void ProcessResponseBody(HttpWebResponse response, string dumpToFile)
        {
            if (response.ContentLength == 0)
            {
                _body = null;
                return;
            }
            _filename = dumpToFile;
            _body = new HttpResponseBody(response, dumpToFile);

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

        [ContextMethod("ПолучитьТелоКакСтроку", "GetBodyAsString")]
        public IValue GetBodyAsString(IValue encoding = null)
        {
            if (_body == null)
                return ValueFactory.Create();

            Encoding enc;
            if (encoding == null)
            {
                if (String.IsNullOrEmpty(_defaultCharset))
                    _defaultCharset = "utf-8";

                enc = Encoding.GetEncoding(_defaultCharset);
            }
            else
                enc = TextEncodingEnum.GetEncoding(encoding);

            using(var reader = new StreamReader(_body.OpenReadStream(), enc))
            {
                return ValueFactory.Create(reader.ReadToEnd());
            }
            
        }

        [ContextMethod("ПолучитьТелоКакДвоичныеДанные", "GetBodyAsBinaryData")]
        public IValue GetBodyAsBinaryData()
        {
            if (_body == null)
                return ValueFactory.Create();

            using (var stream = _body.OpenReadStream())
            {
                var data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
                return new BinaryDataContext(data);
            }
        }

        [ContextMethod("ПолучитьИмяФайлаТела", "GetBodyFileName")]
        public IValue GetBodyFileName()
        {
            if (_filename == null)
                return ValueFactory.Create();

            return ValueFactory.Create(_filename);
        }

        [ContextMethod("Закрыть", "Close")]
        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_body != null)
                _body.Dispose();
        }
    }
}
