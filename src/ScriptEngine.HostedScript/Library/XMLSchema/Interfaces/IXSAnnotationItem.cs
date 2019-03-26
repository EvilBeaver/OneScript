
namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    /// <summary>
    /// Интерфейс для реализации элементов аннотации
    /// </summary>
    /// <see cref="XSAnnotation"/>
    /// <seealso cref="XSDocumentation"/>
    /// <seealso cref="XSAppInfo"/>
    internal interface IXSAnnotationItem : IXSComponent
    {
        string Source { get; set; }
    }
}