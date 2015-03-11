using ScriptEngine.Machine;
using System;

namespace ScriptEngine.HostedScript.Library.Http
{
    class HttpRequestBodyBinary : IHttpRequestBody
    {
        BinaryDataContext _data;

        public HttpRequestBodyBinary(BinaryDataContext data)
        {
            _data = data;
        }

        public IValue GetAsString()
        {
            return ValueFactory.Create();
        }

        public IValue GetAsBinary()
        {
            return _data;
        }

        public IValue GetAsFilename()
        {
            return ValueFactory.Create();
        }

        public System.IO.Stream GetDataStream()
        {
            var bytes = _data.Buffer;
            return new System.IO.MemoryStream(bytes);
        }

        public void Dispose()
        {
            _data = null;
        }
    }
}