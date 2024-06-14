/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using Microsoft.AspNetCore.Http;
using OneScript.Contexts;
using OneScript.StandardLibrary.Collections;
using OneScript.StandardLibrary.Text;
using OneScript.Values;
using OneScript.Web.Server.WebSockets;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OneScript.Web.Server
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

        [ContextProperty("Элементы", "Items", CanWrite = false)]
        public MapImpl Items { get; } = new MapImpl();

        [ContextProperty("ВебСокеты", "WebSockets", CanWrite = false)]
        public WebSocketsManagerWrapper WebSockets => new WebSocketsManagerWrapper(_context.WebSockets);

        [ContextMethod("Прервать", "Abort")]
        public void Abort() => _context.Abort();

        internal HttpContext GetContext() => _context;
    }
}
