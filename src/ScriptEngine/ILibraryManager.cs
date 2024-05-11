using System.Collections.Generic;

namespace ScriptEngine
{
    public interface ILibraryManager
    {
        IEnumerable<ExternalLibraryDef> GetLibraries();
        void InitExternalLibrary(ScriptingEngine runtime, ExternalLibraryDef library);
    }
}