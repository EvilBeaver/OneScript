/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts;
using OneScript.DependencyInjection;
using OneScript.Execution;
using OneScript.Language.SyntaxAnalysis;

namespace ScriptEngine.Machine.Interfaces
{
    public static class InterfaceRegistrationExtensions
    {
        public static IServiceDefinitions EnablePredefinedIterables(this IServiceDefinitions services)
        {
            services.RegisterEnumerable<IPredefinedInterfaceChecker, IterableBslInterfaceChecker>();
            services.RegisterEnumerable<IPredefinedInterfaceChecker, IteratorBslInterfaceChecker>();
            services.RegisterEnumerable<IDirectiveHandler, IterablesModuleAnnotationsHandler>();

            return services;
        }

        public static void AddInterface<T>(this IExecutableModule module, T interfaceData) where T : class
        {
            module.Interfaces[typeof(T)] = interfaceData;
        }
        
        public static T GetInterface<T>(this IExecutableModule module) where T : class
        {
            if (!module.Interfaces.TryGetValue(typeof(T), out var value))
                return null;

            return (T)value;
        }
    }
}