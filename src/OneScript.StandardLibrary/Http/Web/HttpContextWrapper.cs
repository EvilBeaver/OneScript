using Microsoft.AspNetCore.Http;
using OneScript.Contexts;
using OneScript.StandardLibrary.Text;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneScript.StandardLibrary.Http.Web
{
    [ContextClass("HTTPКонтекст", "HTTPContext")]
    public class HttpContextWrapper : AutoContext<HttpContextWrapper>
    {
        internal readonly HttpContext _context;

        public HttpContextWrapper(HttpContext context)
        {
            _context = context;
        }

        [ContextProperty("Запрос", "Request", CanWrite = false)]
        public HttpRequestWrapper Request => new HttpRequestWrapper(_context.Request);

        [ContextProperty("Ответ", "Response", CanWrite = false)]
        public HttpResponseWrapper Response => new HttpResponseWrapper(_context.Response);

        [ContextProperty("Соединение", "Connection", CanWrite = false)]
        public ConnectionInfoWrapper Connection => new ConnectionInfoWrapper(_context.Connection);

        [ContextProperty("ИдентификаторТрассировки", "TraceIdentifier", CanWrite = false)]
        public IValue TraceIdentifier => BslStringValue.Create(_context.TraceIdentifier);

        [ContextProperty("ЗапросПрерван", "RequestAborted", CanWrite = false)]
        public IValue RequestAborted => BslBooleanValue.Create(_context.RequestAborted.IsCancellationRequested);

        [ContextMethod("Прервать", "Abort")]
        public void Abort() => _context.Abort();
    }
}
