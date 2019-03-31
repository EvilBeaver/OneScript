
namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    /// <summary>
    /// Описывает варианты ограничения значения
    /// </summary>
    [EnumerationType("XSConstraint", "ОграничениеЗначенияXS")]
    public enum XSConstraint
    {
        /// <summary>
        /// Используется ограничение по умолчанию
        /// </summary>
        [EnumItem("Default", "ПоУмолчанию")]
        Default,

        /// <summary>
        /// Используется фиксированное значение
        /// </summary>
        [EnumItem("Fixed", "Фиксированное")]
        Fixed
    }
}