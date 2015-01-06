using ScriptEngine.Machine;

namespace ScriptEngine.Environment
{
    public struct ScriptModuleHandle
    {
        internal ModuleImage Module { get; set; }
    }

    public struct LoadedModuleHandle
    {
        internal LoadedModule Module { get; set; }
    }

}
