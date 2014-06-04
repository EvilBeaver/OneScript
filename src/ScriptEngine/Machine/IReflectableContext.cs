using System.Collections.Generic;
using ScriptEngine.Compiler;
using ScriptEngine.Machine;

namespace ScriptEngine.Machine
{
    public interface IReflectableContext
    {
        IEnumerable<VariableInfo> GetProperties();
        IEnumerable<MethodInfo> GetMethods();
    }

}
