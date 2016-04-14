using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime
{
    public class OneScriptProcess : IScriptProcess
    {
        public OneScriptProcess(OneScriptRuntime world)
        {
            World = world;
            Memory = new MachineMemory();
        }

        private OneScriptRuntime World { get; set; }

        public TypeManager TypeManager
        {
            get
            {
                return World.TypeManager;
            }
        }

        internal void Execute(ICompiledModule module, string entryPointName)
        {
            var machine = new OneScriptStackMachine();
            machine.AttachTo(this);
            var mod = (CompiledModule)module;
            machine.SetCode(mod);

            var thread = ScriptThread.Create(this);
            thread.Run(() =>
            {
                var meth = mod.Methods.First(x => x.Name == entryPointName);
                machine.Run(meth);
                return ValueFactory.Create();
            });

        }

        public IRuntimeDataContext RuntimeContext
        {
            get { throw new NotImplementedException(); }
        }

        public MachineMemory Memory
        {
            get;
            private set;
        }

    }
}
