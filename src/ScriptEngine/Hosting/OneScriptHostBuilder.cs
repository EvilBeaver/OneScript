/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Reflection;
using ScriptEngine.Machine;

namespace ScriptEngine.Hosting
{
    public class OneScriptHostBuilder
    {
        public OneScriptHostBuilder()
        {
            Engine = new ScriptingEngine
            {
                Environment = new RuntimeEnvironment()
            };
        }
        
        public ScriptingEngine Engine { get; private set; }

        private RuntimeEnvironment Environment
        {
            get => Engine.Environment;
            set => Engine.Environment = value;
        }

        public OneScriptHostBuilder WithEnvironment(RuntimeEnvironment env)
        {
            Environment = env;
            return this;
        }

        public OneScriptHostBuilder AddAssembly(Assembly asm)
        {
            Engine.AttachAssembly(asm, Environment);
            return this;
        }
        
        public OneScriptHostBuilder AddAssembly(Assembly asm, Predicate<Type> filter)
        {
            Engine.AttachAssembly(asm, Environment, filter);
            return this;
        }

        public OneScriptHostBuilder AddGlobalContext(IAttachableContext context)
        {
            Environment.InjectObject(context);
            GlobalsManager.RegisterInstance(context);
            return this;
        }

        public OneScriptHostBuilder AddGlobalProperty(IValue instance, string name, string alias = null, bool readOnly = true)
        {
            Environment.InjectGlobalProperty(instance, name, readOnly);
            if (alias != null)
            {
                Environment.InjectGlobalProperty(instance, alias, readOnly);
            }

            return this;
        }

        public ScriptingEngine Build()
        {
            Engine.Initialize();
            var inst = Engine;

            Engine = new ScriptingEngine()
            {
                Environment = new RuntimeEnvironment()
            };
            
            return inst;
        }
    }
}