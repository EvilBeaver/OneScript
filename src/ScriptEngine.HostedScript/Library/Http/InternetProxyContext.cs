using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ScriptEngine.HostedScript.Library.Http
{
    [ContextClass("ИнтернетПрокси", "InternetProxy")]
    public class InternetProxyContext : AutoContext<InternetProxyContext>
    {
        WebProxy _proxy;
        NetworkCredential _creds;
        bool _isDefault;
        ArrayImpl _bypassProxyOnAddresses;

        public InternetProxyContext(bool useDefault)
        {
            _isDefault = useDefault;
            if (useDefault)
                _proxy = (WebProxy)WebRequest.GetSystemWebProxy();
            else
                _proxy = new WebProxy();

            _bypassProxyOnAddresses = new ArrayImpl();
        }

        public IWebProxy GetProxy()
        {
            if (!_isDefault)
            {
                _proxy.Credentials = _creds;
            }

            _proxy.BypassList = _bypassProxyOnAddresses.Select(x => x.AsString()).ToArray();

            return _proxy;
        }

        [ContextProperty("Пользователь","User")]
        public string User 
        {
            get
            {
                return _creds.UserName;
            }
            set
            {
                _creds.UserName = value;
            }
        }

        [ContextProperty("Пароль", "Password")]
        public string Password 
        {
            get
            {
                return _creds.Password;
            }
            set
            {
               _creds.Password = value;
            }
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
                return _proxy.BypassProxyOnLocal;
            }
            set
            {
                _proxy.BypassProxyOnLocal = value;
            }
        }
    }
}
