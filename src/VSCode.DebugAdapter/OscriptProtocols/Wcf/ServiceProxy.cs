using System;
using System.ServiceModel;

namespace VSCode.DebugAdapter
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
                    
                    if (ico != null && (ico.State == CommunicationState.Faulted || ico.State == CommunicationState.Closed))
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
