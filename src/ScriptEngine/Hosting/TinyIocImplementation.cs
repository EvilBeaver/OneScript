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
    internal class TinyIocImplementation : IServiceDefinitions, IServiceContainer
    {
        private readonly TinyIoCContainer _container;
        private readonly Dictionary<Type, List<Type>> _multiRegistrations = new Dictionary<Type, List<Type>>();

        #region Registration API

        public TinyIocImplementation()
        {
            _container = new TinyIoCContainer();
        }

        private TinyIocImplementation(TinyIoCContainer container)
        {
            _container = container;
        }
        
        public IServiceContainer CreateContainer()
        {
            foreach (var registration in _multiRegistrations)
            {
                _container.RegisterMultiple(registration.Key, registration.Value).AsMultiInstance();
            }
            
            return this;
        }

        public void Register(Type knownType)
        {
            _container.Register(knownType).AsMultiInstance();
        }

        public void Register(Type interfaceType, Type implementation)
        {
            _container.Register(interfaceType, implementation).AsMultiInstance();
        }

        public void Register<T>() where T : class
        {
            _container.Register<T>().AsMultiInstance();
        }

        public void Register<T>(T instance) where T : class
        {
            _container.Register<T>(instance);
        }

        public void Register<T, TImpl>() where T : class where TImpl : class, T
        {
            _container.Register<T, TImpl>().AsMultiInstance();
        }

        public void Register<T>(Func<IServiceContainer, T> factory) where T : class
        {
            _container.Register<T>((t,p) => factory(this));
        }

        public void RegisterSingleton(Type knownType)
        {
            _container.Register(knownType).AsSingleton();
        }

        public void RegisterSingleton(Type interfaceType, Type implementation)
        {
            _container.Register(interfaceType, implementation).AsSingleton();
        }

        public void RegisterSingleton<T>() where T : class
        {
            _container.Register<T>().AsSingleton();
        }

        public void RegisterSingleton<T>(T instance) where T : class
        {
            _container.Register<T>(instance);
        }

        public void RegisterSingleton<T, TImpl>() where T : class where TImpl : class, T
        {
            _container.Register<T, TImpl>().AsSingleton();
        }

        public void RegisterSingleton<T>(Func<IServiceContainer, T> factory) where T : class
        {
            _container.Register<T>((t, p) => factory(this)).AsSingleton();
        }

        public void RegisterEnumerable<T, TImpl>() where T : class where TImpl : class, T
        {
            if (!_multiRegistrations.TryGetValue(typeof(T), out var list))
            {
                list = new List<Type>();
                _multiRegistrations[typeof(T)] = list;
            }
            
            list.Add(typeof(TImpl));
        }

        #endregion

        #region Resolution API

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
            return new TinyIocImplementation(_container.GetChildContainer());
        }

        #endregion

        public void Dispose()
        {
            _container.Dispose();
        }
    }
}