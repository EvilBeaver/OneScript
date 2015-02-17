using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript.Library.Http
{
    [ContextClass("HTTPЗапрос", "HTTPRequest")]
    public class HttpRequestContext : AutoContext<HttpRequestContext>
    {

        IHttpRequestBody _body;
        static IHttpRequestBody _emptyBody = new HttpRequestBodyUnknown();

        public HttpRequestContext()
        {
            ResourceAddress = "";
            Headers = new MapImpl();
        }

        public void Close()
        {
            SetBody(_emptyBody);
        }

        private void SetBody(IHttpRequestBody newBody)
        {
            _body.Dispose();
            _body = newBody;
        }

        [ContextProperty("АдресРесурса", "ResourceAddress")]
        public string ResourceAddress { get; set; }

        [ContextProperty("Заголовки", "Headers")]
        public MapImpl Headers { get; set; }

        [ContextMethod("УстановитьИмяФайлаТела", "SetBodyFileName")]
        public void SetBodyFileName(string filename)
        {
            SetBody(new HttpRequestBodyFile(filename));
        }

        [ContextMethod("ПолучитьИмяФайлаТела", "GetBodyFileName")]
        public IValue GetBodyFileName()
        {
            return _body.GetAsFilename();
        }

        [ContextMethod("УстановитьТелоИзДвоичныхДанных", "SetBodyFromBinary")]
        public void SetBodyFromBinary(BinaryDataContext data)
        {
            SetBody(new HttpRequestBodyBinary(data));
        }

        [ContextMethod("ПолучитьТелоКакДвоичныеДанные", "GetBodyAsBinary")]
        public IValue GetBodyFromBinary()
        {
            return _body.GetAsBinary();
        }

        [ContextMethod("УстановитьТелоИзСтроки", "SetBodyFromString")]
        public void SetBodyFromString(string data, IValue encoding = null)
        {
            SetBody(new HttpRequestBodyString(data, encoding));
        }

        [ContextMethod("ПолучитьТелоКакСтроку", "GetBodyAsString")]
        public IValue GetBodyAsString()
        {
            return _body.GetAsString();
        }

        [ScriptConstructor]
        public static HttpRequestContext Constructor()
        {
            return new HttpRequestContext();
        }

        [ScriptConstructor(Name = "По адресу ресурса и заголовкам")]
        public static HttpRequestContext Constructor(IValue resource, IValue headers = null)
        {
            var ctx = new HttpRequestContext();
            return ctx;
        }

    }
}
