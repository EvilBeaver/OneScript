using ScriptEngine.Machine;
using System;
using System.IO;

namespace ScriptEngine.HostedScript.Library.Http
{
    class HttpRequestBodyFile : IHttpRequestBody
    {
        private FileStream _bodyOpenedFile;

        public HttpRequestBodyFile(string filename)
        {
            _bodyOpenedFile = new FileStream(filename, FileMode.Open);
        }

        public IValue GetAsString()
        {
            return ValueFactory.Create();
        }

        public IValue GetAsBinary()
        {
            return ValueFactory.Create();
        }

        public IValue GetAsFilename()
        {
            return ValueFactory.Create(_bodyOpenedFile.Name);
        }

        public Stream GetDataStream()
        {
            return _bodyOpenedFile;
        }

        public void Dispose()
        {
            _bodyOpenedFile.Dispose();
        }
    }
}