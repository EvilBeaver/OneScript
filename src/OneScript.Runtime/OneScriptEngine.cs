using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime
{
    public class OneScriptEngine : IScriptEngine
    {
        public OneScriptEngine(OneScriptRuntime world)
        {
            World = world;
        }

        private OneScriptRuntime World { get; set; }

        public TypeManager TypeManager
        {
            get
            {
                return World.TypeManager;
            }
        }

        internal void Execute(ILoadedModule module, string entryPointName)
        {
            throw new NotImplementedException();
        }
    }
}
