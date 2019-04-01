
namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [EnumerationType("XSWhitespaceHandling", "ОбработкаПробельныхСимволовXS")]
    public enum XSWhitespaceHandling
    {
        [EnumItem("Replace", "Заменять")]
        Replace,

        [EnumItem("Collapse", "Сворачивать")]
        Collapse,

        [EnumItem("Preserve", "Сохранять")]
        Preserve
    }
}