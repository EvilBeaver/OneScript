using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime
{
    public class OneScriptStackMachine
    {
        MachineMemory _mem;
        CompiledModule _module;

        public void AttachTo(OneScriptProcess process)
        {
            _mem = process.Memory;
        }

        public void SetCode(CompiledModule module)
        {
            _module = module;
        }

        public void Run(int startAddress)
        {
            throw new NotImplementedException();
        }
    }
}
