/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using OneScript.Commons;
using OneScript.StandardLibrary.Collections;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Http
{
    /// <summary>
    /// Параметры прокси-сервера для доступа в Интернет.
    /// В текущей реализации поддерживается только HTTP прокси. Стандартные методы объекта ИнтернетПрокси из 1С:Предприятие для FTP и SOCKS не реализованы.
    /// </summary>
    [ContextClass("ИнтернетПрокси", "InternetProxy")]
    public class InternetProxyContext : AutoContext<InternetProxyContext>
    {
        private ArrayImpl _bypassProxyOnAddresses;
        private bool _bypassLocal;

        private Dictionary<string, ProxySettings> _proxies = new Dictionary<string, ProxySettings>();

        private class ProxySettings
        {
            public string server;
            public int port;
            public bool useOSAuthentication;
            public IWebProxy proxy;
            public NetworkCredential creds;
        }

        private const string PROTO_HTTP = "http";
        private const string PROTO_HTTPS = "https";

        public InternetProxyContext(bool useDefault)
        {
            var settings = new ProxySettings();
            if (useDefault)
            {
                settings.proxy = WebRequest.GetSystemWebProxy();
                settings.creds = (NetworkCredential)System.Net.CredentialCache.DefaultCredentials;
                if (settings.creds != null)
                    settings.proxy.Credentials = settings.creds;
                else
                    settings.creds = new NetworkCredential();

                _proxies[PROTO_HTTP]  = settings;
                _proxies[PROTO_HTTPS] = settings;
            }
            else
            {
                _bypassLocal = false;
                settings.server = String.Empty;
                settings.creds = new NetworkCredential();

                _proxies[PROTO_HTTP] = settings;
                _proxies[PROTO_HTTPS] = settings;
            }

            _bypassProxyOnAddresses = new ArrayImpl();
        }

        public IWebProxy GetProxy(string protocol)
        {
            var settings = GetSettings(protocol);
            IWebProxy returnProxy;

            if (settings.proxy == null)
            {
                if (String.IsNullOrEmpty(settings.server))
                    throw new RuntimeException("Не заданы настройки прокси-сервера для протокола, используемого в запросе");

                var wp = new WebProxy(settings.server, settings.port);
                wp.Credentials = settings.creds;
                wp.BypassList = _bypassProxyOnAddresses.Select(x => x.AsString()).ToArray();
                wp.BypassProxyOnLocal = _bypassLocal;
                settings.proxy = wp;
                returnProxy = wp;
            }
            else
            {
                returnProxy = _proxies[protocol].proxy;
            }
            
            return returnProxy;
        }

        [ContextMethod("Пользователь","User")]
        public string User(string protocol)
        {
            return GetSettings(protocol).creds.UserName;
        }

        [ContextMethod("Пароль", "Password")]
        public string Password(string protocol)
        {
            return GetSettings(protocol).creds.Password;
        }

        [ContextMethod("Сервер", "Server")]
        public string Server(string protocol)
        {
            return GetSettings(protocol).server;
        }

        [ContextMethod("Порт", "Password")]
        public int Port(string protocol)
        {
            return GetSettings(protocol).port;
        }

        [ContextMethod("Установить", "Set")]
        public void Set(string protocol, string server, int port = 0, string username = "", string password = "", bool useOSAuthentication = true)
        {
            if(!ProtocolNameIsValid(protocol))
                throw RuntimeException.InvalidArgumentValue();

            var settings = new ProxySettings
            {
                server = server,
                port = port,
                creds = new NetworkCredential(username, password),
                useOSAuthentication = useOSAuthentication
            };

            _proxies[protocol] = settings;
        }

        [ContextProperty("НеИспользоватьПроксиДляАдресов","BypassProxyOnAddresses")]
        public ArrayImpl BypassProxyList
        {
            get
            {
                return _bypassProxyOnAddresses;
            }
            set
            {
                _bypassProxyOnAddresses = value;
            }
        }

        [ContextProperty("НеИспользоватьПроксиДляЛокальныхАдресов", "BypassProxyOnLocal")]
        public bool BypassProxyOnLocal
        {
            get
            {
                return _bypassLocal;
            }
            set
            {
                _bypassLocal = value;
            }
        }

        private ProxySettings GetSettings(string protocol)
        {
            if (!ProtocolNameIsValid(protocol))
                throw RuntimeException.InvalidArgumentValue();

            return _proxies[protocol];
        }

        private static bool ProtocolNameIsValid(string protocol)
        {
            return StringComparer.OrdinalIgnoreCase.Compare(protocol, PROTO_HTTP) == 0 || StringComparer.OrdinalIgnoreCase.Compare(protocol, PROTO_HTTPS) == 0;
        }

        [ScriptConstructor(Name = "Формирование неинициализированного объекта")]
        public static InternetProxyContext Constructor()
        {
            return Constructor(ValueFactory.Create(false));
        }

        [ScriptConstructor(Name = "Конструктор для системных настроек прокси")]
        public static InternetProxyContext Constructor(IValue useDefault)
        {
            return new InternetProxyContext(useDefault.AsBoolean());
        }
    }
}
