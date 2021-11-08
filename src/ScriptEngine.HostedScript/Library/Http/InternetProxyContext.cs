/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace ScriptEngine.HostedScript.Library.Http
{
    /// <summary>
    /// Параметры прокси-сервера для доступа в Интернет.
    /// В текущей реализации поддерживается только HTTP прокси. Стандартные методы объекта ИнтернетПрокси из 1С:Предприятие для FTP и SOCKS не реализованы.
    /// </summary>
    [ContextClass("ИнтернетПрокси", "InternetProxy")]
    public class InternetProxyContext : AutoContext<InternetProxyContext>
    {
        private Dictionary<string, IWebProxy> _proxies = new Dictionary<string, IWebProxy>();
        private const string LINUX_ENV_HTTP = "http_proxy";
        private const string LINUX_ENV_HTTPS = "https_proxy";
        private const string LINUX_ENV_NO_PROXY = "no_proxy";

        private ArrayImpl _bypassProxyOnAddresses;
        private bool _bypassLocal;

        public InternetProxyContext(bool useDefault)
        {
            var emptyProxy = new WebProxy();
            _bypassLocal = false;
            _bypassProxyOnAddresses = new ArrayImpl();
            
            if (useDefault)
            {
                if (System.Environment.OSVersion.Platform == System.PlatformID.Unix)
                {
                    var httpEnv = System.Environment.GetEnvironmentVariable(LINUX_ENV_HTTP);
                    _proxies[Uri.UriSchemeHttp] = httpEnv == null ? emptyProxy :
                        _proxies[Uri.UriSchemeHttp] = GetProxyFromEnvironmentVariable(httpEnv);
                    
                    var httpsEnv = System.Environment.GetEnvironmentVariable(LINUX_ENV_HTTPS);
                    _proxies[Uri.UriSchemeHttps] = httpsEnv == null ? emptyProxy :
                        _proxies[Uri.UriSchemeHttps] = GetProxyFromEnvironmentVariable(httpEnv);
                    
                    var noProxy = System.Environment.GetEnvironmentVariable(LINUX_ENV_NO_PROXY) ?? string.Empty;
                    var separator = new[] {',', ' '};
                    var byPassList = noProxy.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var uri in byPassList)
                        _bypassProxyOnAddresses.Add(ValueFactory.Create(uri));
                    foreach (var proxy in _proxies.Values.Cast<WebProxy>())
                        proxy.BypassList = byPassList;
                }
                else
                {
                    var defaultProxy = WebRequest.GetSystemWebProxy();
                    defaultProxy.Credentials = CredentialCache.DefaultNetworkCredentials;

                    _proxies[Uri.UriSchemeHttp] = defaultProxy;
                    _proxies[Uri.UriSchemeHttps] = defaultProxy;
                }
            }
            else
            {
                _proxies[Uri.UriSchemeHttp] = emptyProxy;
                _proxies[Uri.UriSchemeHttps] = emptyProxy;
            }
        }

        private static WebProxy GetProxyFromEnvironmentVariable(string envVariable)
        {
            var proxyBuilder = new UriBuilder(envVariable);
            var proxyUri = new Uri(proxyBuilder.Uri.GetComponents(UriComponents.HttpRequestUrl, UriFormat.UriEscaped));
            var proxyCredentials = proxyBuilder.UserName.Equals(string.Empty)
                ? CredentialCache.DefaultNetworkCredentials
                : GetBasicCredential(proxyUri, proxyBuilder.UserName, proxyBuilder.Password);
            return new WebProxy(proxyUri, true, new string[]{}, proxyCredentials);
        }

        private static ICredentials GetBasicCredential(Uri uri, string username, string password)
        {
            var credential = new NetworkCredential(username, password);
            var cache = new CredentialCache {{uri, "Basic", credential}};
            return cache;
        }

        public IWebProxy GetProxy(string protocol)
        {
            if(!ProtocolNameIsValid(protocol))
                throw RuntimeException.InvalidArgumentValue();
            return _proxies[protocol];
        }

        [ContextMethod("Пользователь","User")]
        public string User(string protocol)
        {
            var proxy = GetProxy(protocol) as WebProxy;
            return proxy?.Credentials.GetCredential(proxy.Address, "Basic").UserName ?? string.Empty;
        }

        [ContextMethod("Пароль", "Password")]
        public string Password(string protocol)
        {
            var proxy = GetProxy(protocol) as WebProxy;
            return proxy?.Credentials.GetCredential(proxy.Address, "Basic").Password ?? string.Empty;
        }

        [ContextMethod("Сервер", "Server")]
        public string Server(string protocol)
        {
            const UriComponents serverComponents = UriComponents.Scheme | UriComponents.Host | UriComponents.PathAndQuery;
            var proxy = GetProxy(protocol) as WebProxy;
            return proxy?.Address.GetComponents(serverComponents, UriFormat.UriEscaped) ?? string.Empty;
        }

        [ContextMethod("Порт", "Port")]
        public int Port(string protocol)
        {
            var proxy = GetProxy(protocol) as WebProxy;
            return proxy?.Address.Port ?? 0;
        }

        [ContextMethod("Установить", "Set")]
        public void Set(string protocol, string server, int port = 0, string username = "", string password = "", bool useOSAuthentication = true)
        {
            protocol = protocol.ToLower();
            if(!ProtocolNameIsValid(protocol))
                throw RuntimeException.InvalidArgumentValue();
            
            var builderServer = new UriBuilder(server);
            if (builderServer.Scheme.Equals(string.Empty))
                builderServer.Scheme = protocol;
            
            if (port != 0)
                builderServer.Port = port;
            
            var proxyCredentials = useOSAuthentication ? CredentialCache.DefaultNetworkCredentials :
               GetBasicCredential(builderServer.Uri, username, password);

            _proxies[protocol] = new WebProxy(builderServer.Uri, _bypassLocal,
                _bypassProxyOnAddresses?.Select(x => x.AsString()).ToArray() ?? new string[] {}, proxyCredentials);
        }

        [ContextProperty("НеИспользоватьПроксиДляАдресов","BypassProxyOnAddresses")]
        public ArrayImpl BypassProxyList
        {
            get => _bypassProxyOnAddresses;
            set
            {
                _bypassProxyOnAddresses = value;
                var bypassList = _bypassProxyOnAddresses?.Select(x => x.AsString()).ToArray() ?? new string[] {};
                foreach (var kv in _proxies)
                    if (kv.Value is WebProxy proxy)
                        proxy.BypassList = bypassList;
            }
        }

        [ContextProperty("НеИспользоватьПроксиДляЛокальныхАдресов", "BypassProxyOnLocal")]
        public bool BypassProxyOnLocal
        {
            get => _bypassLocal;
            set
            {
                _bypassLocal = value;
                foreach (var kv in _proxies)
                    if (kv.Value is WebProxy proxy)
                        proxy.BypassProxyOnLocal = _bypassLocal;
            }
        }

        private static bool ProtocolNameIsValid(string protocol)
        {
            return Uri.UriSchemeHttp.Equals(protocol) || Uri.UriSchemeHttps.Equals(protocol);
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
