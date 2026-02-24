/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts.Converters;
using OneScript.DependencyInjection;

namespace ScriptEngine.Machine.Contexts
{
    /// <summary>
    /// Фабрика конвертеров, которая ведет кеш экземпляров и возвращает имеющийся инстанс, если он уже создан
    /// </summary>
    public class CachedConverterFactory : IValueConverterFactory 
    {
        private readonly IServiceContainer _services;

        public CachedConverterFactory(IServiceContainer services)
        {
            _services = services;
        }

        public IBslValueConverter CreateConverter(Type type)
        {
            if (!type.IsAssignableTo(typeof(IBslValueConverter)))
                throw new ArgumentException($"Type {type} is not assignable to {typeof(IBslValueConverter)}");
            
            return (IBslValueConverter)_services.Resolve(type);
        }
    }
}