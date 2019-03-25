
namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [EnumerationType("XSSimpleTypeVariety", "ВариантПростогоТипаXS")]
    public enum XSSimpleTypeVariety
    {
        [EnumItem("Atomic", "Атомарная")]
        Atomic,

        [EnumItem("Union", "Объединение")]
        Union,

        [EnumItem("List", "Список")]
        List
    }
}