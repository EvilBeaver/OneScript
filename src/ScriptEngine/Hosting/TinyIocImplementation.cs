/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.DependencyInjection;
using TinyIoC;

namespace ScriptEngine.Hosting
{
    public class TinyIocImplementation : IServiceContainer
    {
        private readonly TinyIoCContainer _container;
        private readonly Dictionary<Type, List<Type>> _multiRegistrations = new Dictionary<Type, List<Type>>();
        
        private readonly ISet<Type> _scopedRegistrations = new HashSet<Type>();

        public TinyIocImplementation()
        {
            _container = new TinyIoCContainer();
        }

        private TinyIocImplementation(TinyIoCContainer container, ISet<Type> scopedRegistrations)
        {
            _container = container;
            _scopedRegistrations = scopedRegistrations;
        }
        
        public void RegisterTransient(Type serviceType, Type implementationType)
        {
            _container.Register(serviceType, implementationType).AsMultiInstance();
        }
        
        public void RegisterTransient(Type serviceType, Func<IServiceProvider, object> factory)
        {
            _container.Register(serviceType, (c, p) => factory(new ServiceProviderAdapter(this)));
        }

        public void RegisterSingleton(Type serviceType, Type implementationType)
        {
            _container.Register(serviceType, implementationType).AsSingleton();
        }
        
        public void RegisterSingleton(Type serviceType, object instance)
        {
            _container.Register(serviceType, instance);
        }

        public void RegisterSingleton(Type serviceType, Func<IServiceProvider, object> factory)
        {
            _container.Register(serviceType, (c, p) => factory(new ServiceProviderAdapter(this))).AsSingleton();
        }

        public void RegisterScoped(Type serviceType, Type implementationType)
        {
            _scopedRegistrations.Add(serviceType);
        }

        public void RegisterEnumerable(Type serviceType, Type implementationType)
        {
            if (!_multiRegistrations.TryGetValue(serviceType, out var list))
            {
                list = new List<Type>();
                _multiRegistrations[serviceType] = list;
            }
            
            list.Add(implementationType);
        }

        public void FinalizeRegistrations()
        {
            foreach (var registration in _multiRegistrations)
            {
                _container.RegisterMultiple(registration.Key, registration.Value).AsMultiInstance();
            }
        }

        public object Resolve(Type type)
        {
            return _container.Resolve(type);
        }

        public T Resolve<T>() where T : class
        {
            return (T) Resolve(typeof(T));
        }

        public object TryResolve(Type type)
        {
            var resolved = _container.TryResolve(type, out var instance);
            return resolved ? instance : default;
        }

        public T TryResolve<T>() where T : class
        {
            return (T) TryResolve(typeof(T));
        }

        public IEnumerable<T> ResolveEnumerable<T>() where T : class
        {
            return _container.ResolveAll<T>();
        }

        public IServiceContainer CreateScope()
        {
            var child = new TinyIocImplementation(_container.GetChildContainer(), _scopedRegistrations);
            foreach (var scopedRegistration in _scopedRegistrations)
            {
                child._container.Register(scopedRegistration).AsSingleton();
            }

            return child;
        }

        public void Dispose()
        {
            _container.Dispose();
        }
        
        private class ServiceProviderAdapter : IServiceProvider
        {
            private readonly IServiceContainer _container;

            public ServiceProviderAdapter(IServiceContainer container)
            {
                _container = container;
            }

            public object GetService(Type serviceType)
            {
                return _container.TryResolve(serviceType);
            }
        }
    }
}
