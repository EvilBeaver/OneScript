/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using OneScript.Contexts;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.Net.WebSockets;

namespace OneScript.Web.Server.WebSockets
{
    /// <summary>
    /// Результат выполнения операции Receive вебсокета
    /// </summary>
    [ContextClass("РезультатВебСокетПолучения", "WebSocketReceiveResult")]
    public class WebSocketReceiveResultWrapper: AutoContext<WebSocketReceiveResultWrapper>
    {
        private readonly WebSocketReceiveResult _result;

        public WebSocketReceiveResultWrapper(WebSocketReceiveResult result) 
        {
            _result = result;
        }

        /// <summary>
        /// Указывает причину, по которой удаленная конечная точка инициировала подтверждение закрытия.
        /// </summary>
        [ContextProperty("СостояниеЗакрытия", "CloseStatus", CanWrite = false)]
        public WebSocketCloseStatusWrapper CloseStatus => (WebSocketCloseStatusWrapper)_result.CloseStatus;

        /// <summary>
        /// Возвращает необязательное описание, описывающее, почему удаленная конечная точка инициировала подтверждение закрытия
        /// </summary>
        [ContextProperty("ОписаниеСостоянияЗакрытия", "CloseStatusDescription", CanWrite = false)]
        public IValue CloseStatusDescription
        {
            get
            {
                if (_result.CloseStatusDescription == null)
                    return BslUndefinedValue.Instance;
                else
                    return BslStringValue.Create(_result.CloseStatusDescription);
            }
        }

        /// <summary>
        /// Указывает число байт, полученных WebSocket
        /// </summary>
        [ContextProperty("Количество", "Count", CanWrite = false)]
        public IValue Count => BslNumericValue.Create(_result.Count);

        /// <summary>
        /// Указывает, было ли сообщение получено полностью
        /// </summary>
        [ContextProperty("КонецСообщения", "EndOfMessage", CanWrite = false)]
        public IValue EndOfMessage => BslBooleanValue.Create(_result.EndOfMessage);

        /// <summary>
        /// Указывает, является ли текущее сообщение сообщением UTF-8 или двоичным сообщением
        /// </summary>
        [ContextProperty("ТипСообщения", "MessageType", CanWrite = false)]
        public WebSocketMessageTypeWrapper MessageType => (WebSocketMessageTypeWrapper)_result.MessageType;
    }
}
