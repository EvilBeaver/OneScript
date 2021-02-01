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
        void Register(Type knownType);
        void Register(Type interfaceType, Type implementation);
        void Register<T>();
        void Register<T>(T instance);
        void Register<T,TImpl>();
        void Register<T>(Func<IServiceContainer, T> factory);
        
        void RegisterSingleton(Type knownType);
        void RegisterSingleton(Type interfaceType, Type implementation);
        void RegisterSingleton<T>();
        void RegisterSingleton<T>(T instance);
        void RegisterSingleton<T,TImpl>();
        void RegisterSingleton<T>(Func<IServiceContainer, T> factory);
    }
}