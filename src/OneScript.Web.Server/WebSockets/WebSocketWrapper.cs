/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using OneScript.Contexts;
using OneScript.StandardLibrary.Binary;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.IO;
using System.Net.WebSockets;
using System.Text;

namespace OneScript.Web.Server.WebSockets
{
    /// <summary>
    /// ВебСокет подключение
    /// </summary>
    [ContextClass("ВебСокет", "WebSocket")]
    public class WebSocketWrapper: AutoContext<WebSocketWrapper>
    {
        private readonly WebSocket _webSocket;

        public WebSocketWrapper(WebSocket webSocket) 
        {
            _webSocket = webSocket;
        }

        /// <summary>
        /// Указывает причину, по которой удаленная конечная точка инициировала подтверждение закрытия
        /// </summary>
        [ContextProperty("СостояниеЗакрытия", "CloseStatus", CanWrite = false)]
        public WebSocketCloseStatusWrapper CloseStatus => (WebSocketCloseStatusWrapper)_webSocket.CloseStatus;

        /// <summary>
        /// Позволяет удаленной конечной точке описать причину закрытия подключения
        /// </summary>
        [ContextProperty("ОписаниеСостоянияЗакрытия", "CloseStatusDescription", CanWrite = false)]
        public IValue CloseStatusDescription
        {
            get
            {
                if (_webSocket.CloseStatusDescription == null)
                    return BslUndefinedValue.Instance;
                else
                    return BslStringValue.Create(_webSocket.CloseStatusDescription);
            }
        }

        /// <summary>
        /// Возвращает текущее состояние соединения WebSocket
        /// </summary>
        [ContextProperty("Состояние", "Status", CanWrite = false)]
        public WebSocketStateWrapper Status => (WebSocketStateWrapper)_webSocket.State;

        /// <summary>
        /// Возвращает подпротокол, который был согласован во время подтверждения открытия
        /// </summary>
        [ContextProperty("Протокол", "Protocol", CanWrite = false)]
        public IValue Protocol
            => _webSocket.SubProtocol == null ? BslUndefinedValue.Instance : BslStringValue.Create(_webSocket.SubProtocol);

        /// <summary>
        /// Отменяет соединение WebSocket и отменяет все ожидающие операции ввода-вывода
        /// </summary>
        [ContextMethod("Прервать", "Abort")]
        public void Abort() => _webSocket.Abort();

        /// <summary>
        /// Закрывает подключение WebSocket в качестве асинхронной операции, используя подтверждение закрытия, которое определено в разделе 7 спецификации протокола WebSocket
        /// </summary>
        /// <param name="status">Статус закрытия</param>
        /// <param name="statusDescription">Описание причины закрытия</param>
        [ContextMethod("Закрыть", "Close")]
        public void Close(WebSocketCloseStatusWrapper status, IValue statusDescription)
        {
            var desc = statusDescription is BslUndefinedValue ? null : statusDescription.AsString();

            _webSocket.CloseAsync((WebSocketCloseStatus)status, desc, default).Wait();
        }

        /// <summary>
        /// Инициирует или завершает подтверждение закрытия, определенное в разделе 7 спецификации протокола WebSocket
        /// </summary>
        /// <param name="status">Статус закрытия</param>
        /// <param name="statusDescription">Описание причины закрытия</param>
        [ContextMethod("ЗакрытьВыходнойПоток", "CloseOutput")]
        public void CloseOutput(WebSocketCloseStatusWrapper status, IValue statusDescription)
        {
            var desc = statusDescription is BslUndefinedValue ? null : statusDescription.AsString();

            _webSocket.CloseOutputAsync((WebSocketCloseStatus)status, desc, default).Wait();
        }

        /// <summary>
        /// Получает данные через WebSocket соединениe
        /// </summary>
        /// <param name="buffer">Буфер двоичных данных</param>
        /// <returns></returns>
        [ContextMethod("Получить", "Receive")]
        public WebSocketReceiveResultWrapper Receive(BinaryDataBuffer buffer)
        {
            var result = _webSocket.ReceiveAsync(buffer.Bytes, default).Result;

            return new WebSocketReceiveResultWrapper(result);
        }

        /// <summary>
        /// Получает строку через WebSocket соединение
        /// </summary>
        /// <returns></returns>
        [ContextMethod("ПолучитьСтроку", "ReceiveString")]
        public BslStringValue ReceiveString()
        {
            var buffer = new byte[1024];
            using var stream = new MemoryStream();

            WebSocketReceiveResult result;
            do
            {
                result = _webSocket.ReceiveAsync(buffer, default).Result;
                stream.Write(buffer);
            }
            while (!result.EndOfMessage);

            var data = stream.GetBuffer();

            return BslStringValue.Create(Encoding.UTF8.GetString(data));
        }

        /// <summary>
        /// Получает двоичные данные через WebSocket соединение
        /// </summary>
        /// <returns></returns>
        [ContextMethod("ПолучитьДвоичныеДанные", "ReceiveBinary")]
        public byte[] ReceiveBinary()
        {
            var buffer = new byte[1024];
            using var stream = new MemoryStream();

            WebSocketReceiveResult result;
            do
            {
                result = _webSocket.ReceiveAsync(buffer, default).Result;
                stream.Write(buffer);
            }
            while (!result.EndOfMessage);

            var data = stream.GetBuffer();

            return data;
        }

        /// <summary>
        /// Отправляет данные из буфера в подключение WebSocket
        /// </summary>
        /// <param name="buffer">Буфер двоичных данных</param>
        /// <param name="messageType">Тип сообщения</param>
        /// <param name="flag">Флаг сообщения</param>
        [ContextMethod("Отправить", "Send")]
        public void Send(BinaryDataBuffer buffer, WebSocketMessageTypeWrapper messageType, WebSocketMessageFlagsWrapper flag)
        {
            _webSocket.SendAsync(buffer.Bytes, (WebSocketMessageType)messageType, (WebSocketMessageFlags)flag, default).AsTask().Wait();
        }

        /// <summary>
        /// Отправляет строку в подключение WebSocket
        /// </summary>
        /// <param name="value">Строка - отправляемые данные</param>
        [ContextMethod("ОтправитьСтроку", "SendString")]
        public void SendString(IValue value)
        {
            _webSocket.SendAsync(Encoding.UTF8.GetBytes(value.AsString()), WebSocketMessageType.Text, WebSocketMessageFlags.EndOfMessage, default).AsTask().Wait();
        }

        /// <summary>
        /// Отправляет двоичные данные в подключение WebSocket
        /// </summary>
        /// <param name="value">ДвоичныеДанные - отправляемые данные</param>
        [ContextMethod("ОтправитьДвоичныеДанные", "SendBinary")]
        public void SendBinary(byte[] value)
        {
            _webSocket.SendAsync(value, WebSocketMessageType.Binary, WebSocketMessageFlags.EndOfMessage, default).AsTask().Wait();
        }
    }
}
