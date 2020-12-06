/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Reflection;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Hosting
{
    public static class EngineBuilderExtensions
    {
        public static IEngineBuilder WithEnvironment(this IEngineBuilder b, RuntimeEnvironment env)
        {
            b.Environment = env;
            return b;
        }

        public static IEngineBuilder WithTypes(this IEngineBuilder b, ITypeManager typeManager)
        {
            b.TypeManager = typeManager;
            return b;
        }
        
        public static IEngineBuilder WithGlobals(this IEngineBuilder b, IGlobalsManager globals)
        {
            b.GlobalInstances = globals;
            return b;
        }

        public static IEngineBuilder AddAssembly(this IEngineBuilder b, Assembly asm, Predicate<Type> filter = null)
        {
            var discoverer = new ContextDiscoverer(b.TypeManager, b.GlobalInstances);
            discoverer.DiscoverClasses(asm, filter);
            discoverer.DiscoverGlobalContexts(b.Environment, asm, filter);
            return b;
        }
        
        public static IEngineBuilder AddGlobalContext(this IEngineBuilder b, IAttachableContext context)
        {
            b.Environment.InjectObject(context);
            b.GlobalInstances.RegisterInstance(context);
            return b;
        }

        public static IEngineBuilder AddGlobalProperty(this IEngineBuilder b, IValue instance, string name, string alias = null, bool readOnly = true)
        {
            b.Environment.InjectGlobalProperty(instance, name, readOnly);
            if (alias != null)
            {
                b.Environment.InjectGlobalProperty(instance, alias, readOnly);
            }

            return b;
        }

        public static IEngineBuilder UseCompiler(this IEngineBuilder b, ICompilerServiceFactory factory)
        {
            b.CompilerFactory = factory;
            return b;
        }
    }
}