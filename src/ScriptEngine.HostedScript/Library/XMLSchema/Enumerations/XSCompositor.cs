using System.Xml.Schema;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    /// <summary>
    /// Вид группы модели
    /// </summary>
    /// <see cref="XmlSchemaGroupBase"/>
    [EnumerationType("XSCompositor", "ВидГруппыМоделиXS")]
    public enum XSCompositor
    {
        /// <summary>
        /// Требует наличия элементов группы без требования последовательности
        /// </summary>
        /// <see cref="XmlSchemaAll"/>
        [EnumItem("All", "Все")]
        All,

        /// <summary>
        /// Требует наличия только одного из элементов группы
        /// </summary>
        /// <see cref="XmlSchemaChoice"/>
        [EnumItem("Choice", "Выбор")]
        Choice,

        /// <summary>
        /// Требует чтобы элементы следовали в указанной последовательности
        /// </summary>
        /// <see cref="XmlSchemaSequence"/>
        [EnumItem("Sequence", "Последовательность")]
        Sequence
    }
}