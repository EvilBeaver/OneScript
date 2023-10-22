using System;

namespace ScriptEngine.Persistence
{
    [Serializable]
    public struct ExportedSymbol
    {
        public string SymbolicName;
        public int Index;
    }
}