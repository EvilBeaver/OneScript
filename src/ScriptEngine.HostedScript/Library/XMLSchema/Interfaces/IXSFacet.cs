
namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    public interface IXSFacet : IXSAnnotated
    {
        XSSimpleTypeDefinition SimpleTypeDefinition { get; }
        string LexicalValue { get; set; }
        bool Fixed { get; set; }
    }
}