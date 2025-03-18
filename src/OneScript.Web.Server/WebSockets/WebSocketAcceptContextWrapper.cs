/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using Microsoft.AspNetCore.Http;
using OneScript.Contexts;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;

namespace OneScript.Web.Server.WebSockets
{
    /// <summary>
    /// Настройки согласования вебсокет подключения
    /// </summary>
    [ContextClass("КонтекстПодключенияВебСокета", "WebSocketsAcceptContext")]
    public class WebSocketAcceptContextWrapper : AutoContext<WebSocketAcceptContextWrapper>
    {
        internal readonly WebSocketAcceptContext _context = new();

        /// <summary>
        /// Согласовываемый субпротокол
        /// </summary>
        [ContextProperty("Протокол", "Protocol")]
        public IValue Protocol
        {
            get
            {
                return _context.SubProtocol == null ? BslUndefinedValue.Instance : BslStringValue.Create(_context.SubProtocol);
            }
            set
            {
                _context.SubProtocol = value is BslUndefinedValue ? null : value.AsString();
            }
        }

        /// <summary>
        /// Периодичность отправки KeepAlive пакетов соединения (в секундах)
        /// </summary>
        [ContextProperty("KeepAlive")]
        public IValue KeepAlive
        {
            get
            {
                if (_context.KeepAliveInterval.HasValue)
                    return BslNumericValue.Create((decimal)_context.KeepAliveInterval.Value.TotalSeconds);
                else
                    return BslUndefinedValue.Instance;
            }
            set
            {
                if (value is BslUndefinedValue)
                    _context.KeepAliveInterval = null;
                else
                    _context.KeepAliveInterval = TimeSpan.FromSeconds((double)value.AsNumber());
            }
        }

        /// <summary>
        /// Включает поддержку расширения WebSocket permessage-deflate.
        /// 
        /// Имейте в виду, что включение сжатия через зашифрованные подключения делает приложение подверженным атакам типа CRIME/BREACH.
        /// Настоятельно рекомендуется отключить сжатие при отправке данных, содержащих секреты, указав при отправке DisableCompression таких сообщений.
        /// </summary>
        [ContextProperty("ВключитьСжатие", "EnableCompression")]
        public IValue EnableCompression
        {
            get => BslBooleanValue.Create(_context.DangerousEnableCompression);
            set => _context.DangerousEnableCompression = value.AsBoolean();
        }

        /// <summary>
        /// Отключает переключение контекста сервера при использовании сжатия. 
        /// Этот параметр снижает нагрузку на память при сжатии за счет потенциально худшего коэффициента сжатия
        /// </summary>
        [ContextProperty("ОтключитьПерехватСерверногоКонтекста", "DisableServerContextTakeover")]
        public IValue DisableServerContextTakeover
        {
            get => BslBooleanValue.Create(_context.DisableServerContextTakeover);
            set => _context.DisableServerContextTakeover = value.AsBoolean();
        }

        /// <summary>
        /// Задает максимальный log.2 размера скользящего окна LZ77, который можно использовать для сжатия. 
        /// Этот параметр снижает нагрузку на память при сжатии за счет потенциально худшего коэффициента сжатия
        /// </summary>
        [ContextProperty("МаксимумWindowBits", "ServerMaxWindowBits")]
        public IValue ServerMaxWindowBits
        {
            get => BslNumericValue.Create(_context.ServerMaxWindowBits);
            set => _context.ServerMaxWindowBits = (int)value.AsNumber();
        }

        internal WebSocketAcceptContext GetContext() => _context;
    }
}
