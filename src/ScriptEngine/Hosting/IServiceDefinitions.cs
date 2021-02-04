/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace ScriptEngine.Hosting
{
    public interface IServiceDefinitions
    {
        IServiceContainer CreateContainer();
        
        void Register(Type knownType);
        void Register(Type interfaceType, Type implementation);
        void Register<T>() where T : class;
        void Register<T>(T instance) where T : class;
        void Register<T,TImpl>() 
            where T : class 
            where TImpl : class, T;
        
        void Register<T>(Func<IServiceContainer, T> factory) where T : class;
        
        void RegisterSingleton(Type knownType);
        void RegisterSingleton(Type interfaceType, Type implementation);
        void RegisterSingleton<T>() where T : class;
        void RegisterSingleton<T>(T instance) where T : class;
        void RegisterSingleton<T,TImpl>() 
            where T : class
            where TImpl : class, T;
        
        void RegisterSingleton<T>(Func<IServiceContainer, T> factory) where T : class;
        
        void RegisterEnumerable<T, TImpl>() 
            where T : class 
            where TImpl : class,T;
    }
}