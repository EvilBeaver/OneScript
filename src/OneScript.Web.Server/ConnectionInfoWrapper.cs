/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using Microsoft.AspNetCore.Http;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.Net;
using OneScript.Localization;

namespace OneScript.Web.Server
{
    /// <summary>
    /// Сетвевая информация и контексте подключения
    /// </summary>
    [ContextClass("ИнформацияОСоединении", "ConnectionInfo")]
    public class ConnectionInfoWrapper: AutoContext<ConnectionInfoWrapper>
    {
        private readonly ConnectionInfo _connectionInfo;

        public ConnectionInfoWrapper(ConnectionInfo connectionInfo)
        {
            _connectionInfo = connectionInfo;
        }

        /// <summary>
        /// Идентификатор подключения
        /// </summary>
        [ContextProperty("Идентификатор", "Id")]
        public string Id
        {
            get => _connectionInfo.Id;
            set => _connectionInfo.Id = value;
        }

        /// <summary>
        /// IP адрес клиента
        /// </summary>
        [ContextProperty("УдаленныйIpАдрес", "RemoteIpAddress")]
        public string RemoteIpAddress
        {
            get => _connectionInfo.RemoteIpAddress?.ToString() ?? "";
            set
            {
                if (IPAddress.TryParse(value, out var ip))
                    _connectionInfo.RemoteIpAddress = ip;
                else
                    throw new RuntimeException(new BilingualString($"Ошибка разбора IP адреса: {value}", $"Failed to parse IP address: {value}"));
            }
        }

        /// <summary>
        /// Порт клиента
        /// </summary>
        [ContextProperty("УдаленныйПорт", "RemotePort")]
        public IValue RemotePort
        {
            get => BslNumericValue.Create(_connectionInfo.RemotePort);
            set
            {
                _connectionInfo.RemotePort = (int)value.AsNumber();
            }
        }

        /// <summary>
        /// IP адрес сервера
        /// </summary>
        [ContextProperty("ЛокальныйIpАдрес", "LocalIpAddress")]
        public string LocalIpAddress
        {
            get => _connectionInfo.LocalIpAddress?.ToString() ?? "";
            set
            {
                if (IPAddress.TryParse(value, out var ip))
                    _connectionInfo.LocalIpAddress = ip;
                else
                    throw new RuntimeException(new BilingualString($"Ошибка разбора IP адреса: {value}", $"Failed to parse IP address: {value}"));
            }
        }

        /// <summary>
        /// Порт сервера
        /// </summary>
        [ContextProperty("ЛокальныйПорт", "LocalPort")]
        public IValue LocalPort
        {
            get => BslNumericValue.Create(_connectionInfo.LocalPort);
            set
            {
                _connectionInfo.LocalPort = (int)value.AsNumber();
            }
        }
    }
}
