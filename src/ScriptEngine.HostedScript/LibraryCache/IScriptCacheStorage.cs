using OneScript.Execution;

namespace ScriptEngine.HostedScript.LibraryCache
{
    public interface IScriptCacheStorage
    {
        void Store(string key, IExecutableModule module);

        IExecutableModule Load(string key);
        
        bool Exists(string key);
        
        bool IsValid(string key);
        
        void Delete(string key);
        
        bool CanStore(IExecutableModule module);
    }
}