/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.DependencyInjection;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Native.Compiler;

namespace OneScript.Native.Extensions
{
    public static class ServiceRegistrationExtensions
    {
        public static IServiceDefinitions UseNativeRuntime(this IServiceDefinitions services)
        {
            services.RegisterEnumerable<IDirectiveHandler, NativeRuntimeAnnotationHandler>();
            return services;
        }
    }
}