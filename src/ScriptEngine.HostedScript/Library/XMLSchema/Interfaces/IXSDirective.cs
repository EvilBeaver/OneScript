
namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    public interface IXSDirective : IXSComponent
    {
        XMLSchema ResolvedSchema { get; set; }
        string SchemaLocation { get; set; }
    }
}
