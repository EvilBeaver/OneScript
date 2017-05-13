using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace DebugServer
{
    internal class ServiceProxy<T> where T : class
    {
        private T _instance;
        private readonly Func<T> _factory;

        private readonly object _lock = new object();

        public ServiceProxy(Func<T> creationMethod)
        {
            _factory = creationMethod;
        }

        public T Instance
        {
            get
            {
                lock (_lock)
                {
                    var ico = (ICommunicationObject) _instance;
                    if (ico?.State == CommunicationState.Faulted)
                    {
                        ico.Abort();
                        _instance = null;
                    }

                    if (_instance == null)
                    {
                        _instance = _factory();
                    }
                }

                return _instance;
            }
        }
    }
}
