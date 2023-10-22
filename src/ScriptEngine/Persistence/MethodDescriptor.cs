using System;
using OneScript.Contexts;
using ScriptEngine.Machine;

namespace ScriptEngine.Persistence
{
    [Serializable]
    public struct MethodDescriptor
    {
        public MethodSignature Signature;
        public VariablesFrame Variables;
        public int EntryPoint;
    }
}