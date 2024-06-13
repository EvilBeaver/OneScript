using Microsoft.AspNetCore.Http;
using OneScript.Contexts;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.Net;

namespace OneScript.Web.Server
{
    [ContextClass("ИнформацияОСоединении", "ConnectionInfo")]
    public class ConnectionInfoWrapper: AutoContext<ConnectionInfoWrapper>
    {
        private readonly ConnectionInfo _connectionInfo;

        public ConnectionInfoWrapper(ConnectionInfo connectionInfo)
        {
            _connectionInfo = connectionInfo;
        }

        [ContextProperty("Идентификатор", "Id")]
        public IValue Id
        {
            get => BslStringValue.Create(_connectionInfo.Id);
            set
            {
                _connectionInfo.Id = value.AsString();
            }
        }

        [ContextProperty("УдаленныйIpАдрес", "RemoteIpAddress")]
        public IValue RemoteIpAddress
        {
            get => BslStringValue.Create(_connectionInfo.RemoteIpAddress.ToString());
            set
            {
                _connectionInfo.RemoteIpAddress = IPAddress.Parse(value.AsString());
            }
        }

        [ContextProperty("УдаленныйПорт", "RemotePort")]
        public IValue RemotePort
        {
            get => BslNumericValue.Create(_connectionInfo.RemotePort);
            set
            {
                _connectionInfo.RemotePort = (int)value.AsNumber();
            }
        }

        [ContextProperty("ЛокальныйIpАдрес", "LocalIpAddress")]
        public IValue LocalIpAddress
        {
            get => BslStringValue.Create(_connectionInfo.LocalIpAddress.ToString());
            set
            {
                _connectionInfo.LocalIpAddress = IPAddress.Parse(value.AsString());
            }
        }

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
