/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OneScript.Contexts;
using OneScript.StandardLibrary.Collections;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Linq;
using System.Net.WebSockets;

namespace OneScript.Web.Server.WebSockets
{
    /// <summary>
    /// Менеджер, управляющий установкой вебсокет соединения для HTTP запросов
    /// </summary>
    [ContextClass("МенеджерВебСокетов", "WebSocketsManager")]
    public class WebSocketsManagerWrapper : AutoContext<WebSocketsManagerWrapper>
    {
        internal readonly WebSocketManager _manager;

        public WebSocketsManagerWrapper(WebSocketManager manager)
        {
            _manager = manager;
        }

        /// <summary>
        /// Возвращает значение, указывающее, является ли запрос запросом на создание WebSocket
        /// </summary>
        [ContextProperty("ЭтоВебСокетЗапрос", "IsWebSocketRequest", CanWrite = false)]
        public IValue RequestAborted => BslBooleanValue.Create(_manager.IsWebSocketRequest);

        /// <summary>
        /// Возвращает список запрошенных подпротоколов WebSocket
        /// </summary>
        [ContextProperty("ЗапрошенныеПротоколы", "RequestedProtocols", CanWrite = false)]
        public ArrayImpl RequestedProtocols
            => new ArrayImpl(_manager.WebSocketRequestedProtocols.Select(c => BslStringValue.Create(c)));

        /// <summary>
        /// Принимает WebSocket подключение
        /// </summary>
        /// <param name="context">Настройки согласования подключения</param>
        /// <returns></returns>
        [ContextMethod("ПодключитьВебСокет", "AcceptWebSocket")]
        public WebSocketWrapper AcceptWebSocket(WebSocketAcceptContextWrapper context = null)
        {
            WebSocket webSocket;

            if (context == null)
                webSocket = _manager.AcceptWebSocketAsync().Result;
            else
                webSocket = _manager.AcceptWebSocketAsync(context.GetContext()).Result;

            return new WebSocketWrapper(webSocket);
        }
    }
}
