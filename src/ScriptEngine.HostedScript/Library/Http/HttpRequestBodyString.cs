using ScriptEngine.Machine;
using System;
using System.Text;

namespace ScriptEngine.HostedScript.Library.Http
{
    class HttpRequestBodyString : IHttpRequestBody
    {
        string _data;
        Encoding _encoding;
        
        public HttpRequestBodyString(string body, IValue encoding = null)
        {
            _data = body;
            if (encoding == null)
                _encoding = new UTF8Encoding(true);
            else
                _encoding = TextEncodingEnum.GetEncoding(encoding);
        }

        public IValue GetAsString()
        {
            return ValueFactory.Create(_data);
        }

        public IValue GetAsBinary()
        {
            return ValueFactory.Create();
        }

        public IValue GetAsFilename()
        {
            return ValueFactory.Create();
        }

        public System.IO.Stream GetDataStream()
        {
            var bytes = _encoding.GetBytes(_data);
            return new System.IO.MemoryStream(bytes);
        }

        public void Dispose()
        {
            _data = null;
            _encoding = null;
        }
    }
}