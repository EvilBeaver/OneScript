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
using System.Net;

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
        public IValue Id
        {
            get => BslStringValue.Create(_connectionInfo.Id);
            set
            {
                _connectionInfo.Id = value.AsString();
            }
        }

        /// <summary>
        /// IP адрес клиента
        /// </summary>
        [ContextProperty("УдаленныйIpАдрес", "RemoteIpAddress")]
        public IValue RemoteIpAddress
        {
            get => BslStringValue.Create(_connectionInfo.RemoteIpAddress.ToString());
            set
            {
                _connectionInfo.RemoteIpAddress = IPAddress.Parse(value.AsString());
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
        public IValue LocalIpAddress
        {
            get => BslStringValue.Create(_connectionInfo.LocalIpAddress.ToString());
            set
            {
                _connectionInfo.LocalIpAddress = IPAddress.Parse(value.AsString());
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
