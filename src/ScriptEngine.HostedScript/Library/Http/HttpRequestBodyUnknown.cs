using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript.Library.Http
{
    class HttpRequestBodyUnknown : IHttpRequestBody
    {

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
            return ValueFactory.Create();
        }

        public void Dispose()
        {
            ;
        }

        public System.IO.Stream GetDataStream()
        {
            return null;
        }
    }
}