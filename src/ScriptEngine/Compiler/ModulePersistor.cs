using ScriptEngine.Machine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ScriptEngine.Compiler
{
    public class ModulePersistor
    {
        IFormatter _formatter;

        public ModulePersistor (IFormatter format)
	    {
            _formatter = format;
	    }

        public void Save(ModuleHandle module, Stream output)
        {
            _formatter.Serialize(output, FromHandle(module));
        }

        public ModuleHandle Read(Stream input)
        {
            var moduleImage = (ModuleImage)_formatter.Deserialize(input);
            return new ModuleHandle()
            {
                Module = moduleImage
            };
        }

        private ModuleImage FromHandle(ModuleHandle module)
        {
            return module.Module;
        }
    }
}
