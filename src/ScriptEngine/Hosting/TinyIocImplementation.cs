/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using TinyIoC;

namespace ScriptEngine.Hosting
{
    public class TinyIocImplementation /*: IServiceDefinitions, IServiceContainer*/
    {
        private readonly TinyIoCContainer _container = new TinyIoCContainer();

        #region Registration API

        public IServiceContainer CreateContainer()
        {
            throw new NotImplementedException();
        }

        public void Register(Type knownType)
        {
            throw new NotImplementedException();
        }

        public void Register(Type interfaceType, Type implementation)
        {
            throw new NotImplementedException();
        }

        public void Register<T>()
        {
            throw new NotImplementedException();
        }

        public void Register<T>(T instance)
        {
            throw new NotImplementedException();
        }

        public void Register<T, TImpl>()
        {
            throw new NotImplementedException();
        }

        public void Register<T>(Func<IServiceContainer, T> factory)
        {
            throw new NotImplementedException();
        }

        public void RegisterSingleton(Type knownType)
        {
            throw new NotImplementedException();
        }

        public void RegisterSingleton(Type interfaceType, Type implementation)
        {
            throw new NotImplementedException();
        }

        public void RegisterSingleton<T>()
        {
            throw new NotImplementedException();
        }

        public void RegisterSingleton<T>(T instance)
        {
            throw new NotImplementedException();
        }

        public void RegisterSingleton<T, TImpl>()
        {
            throw new NotImplementedException();
        }

        public void RegisterSingleton<T>(Func<IServiceContainer, T> factory)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Resolution API

        public object Resolve(Type type)
        {
            throw new NotImplementedException();
        }

        public T Resolve<T>()
        {
            throw new NotImplementedException();
        }

        public object TryResolve(Type type)
        {
            throw new NotImplementedException();
        }

        public T TryResolve<T>()
        {
            throw new NotImplementedException();
        }

        public IServiceContainer CreateScope()
        {
            throw new NotImplementedException();
        }

        #endregion

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}