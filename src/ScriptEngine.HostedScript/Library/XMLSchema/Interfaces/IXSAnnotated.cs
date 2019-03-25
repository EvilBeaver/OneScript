namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    public interface IXSAnnotated : IXSComponent
    {
        new XSAnnotation Annotation { get; set; }
    }
}
