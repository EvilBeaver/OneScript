using OneScript.Language.Sources;

namespace ScriptEngine.Serialization
{
    public interface ICodeSourceImageProvider
    {
        string ProviderKey { get; }
        
        bool CanHandle(ICodeSource source);
        
        CodeSourceImage CreateImage(ICodeSource source);
        
        bool TryRestore(CodeSourceImage image, out ICodeSource source, out string error);
    }
}
