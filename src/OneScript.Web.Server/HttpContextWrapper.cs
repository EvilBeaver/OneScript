/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using Microsoft.AspNetCore.Http;
using OneScript.Contexts;
using OneScript.StandardLibrary.Collections;
using OneScript.Web.Server.WebSockets;
using ScriptEngine.Machine.Contexts;
using OneScript.Types;

namespace OneScript.Web.Server
{
    [ContextClass("HTTPКонтекст", "HTTPContext")]
    public class HttpContextWrapper : AutoContext<HttpContextWrapper>
    {
        private readonly ITypeManager _typeManager;
        private readonly HttpContext _context;
        private readonly PropertyWrappersCollection _wrappers = new();

        public HttpContextWrapper(ITypeManager typeManager, HttpContext context)
        {
            _typeManager = typeManager;
            _context = context;
        }

        [ContextProperty("Запрос", "Request", CanWrite = false)]
        public HttpRequestWrapper Request =>
            _wrappers.Get(nameof(Request), () => new HttpRequestWrapper(_context.Request));

        [ContextProperty("Ответ", "Response", CanWrite = false)]
        public HttpResponseWrapper Response =>
            _wrappers.Get(nameof(Response), () => new HttpResponseWrapper(_context.Response));

        [ContextProperty("Соединение", "Connection", CanWrite = false)]
        public ConnectionInfoWrapper Connection =>
            _wrappers.Get(nameof(Connection), () => new ConnectionInfoWrapper(_context.Connection));

        [ContextProperty("ИдентификаторТрассировки", "TraceIdentifier", CanWrite = false)]
        public string TraceIdentifier => _context.TraceIdentifier;

        [ContextProperty("ЗапросПрерван", "RequestAborted", CanWrite = false)]
        public bool RequestAborted => _context.RequestAborted.IsCancellationRequested;

        [ContextProperty("Данные", "Data", CanWrite = false)]
        public AutoCollectionContext<MapImpl, KeyAndValueImpl> Data => _wrappers.Get(nameof(Data), () =>
            MapWrapper<object, object>.Create(_typeManager, _context.Items));

        [ContextProperty("ВебСокеты", "WebSockets", CanWrite = false)]
        public WebSocketsManagerWrapper WebSockets => _wrappers.Get(nameof(WebSockets), () =>
            new WebSocketsManagerWrapper(_context.WebSockets));

        [ContextMethod("Прервать", "Abort")]
        public void Abort() => _context.Abort();

        internal HttpContext GetContext() => _context;
    }
}