using System.Xml.Schema;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("ОбъединениеЗавершенностиСхемыXS", "XSSchemaFinalUnion")]
    public class XSSchemaFinalUnion : AutoContext<XSSchemaFinalUnion>
    {
        private ArrayImpl _values;

        private bool Contains(XmlSchemaDerivationMethod _value)
        {
            XSSchemaFinal _enumValue = EnumerationXSSchemaFinal.FromNativeValue(_value);
            IValue _idx = _values.Find(_enumValue);
            return (_idx.DataType != DataType.Undefined);
        }

        internal XSSchemaFinalUnion() => _values = ArrayImpl.Constructor();

        #region OneScript

        #region Properties

        [ContextProperty("Все", "All")]
        public bool All => Contains(XmlSchemaDerivationMethod.All);

        [ContextProperty("Объединение", "Union")]
        public bool Union => Contains(XmlSchemaDerivationMethod.Union);

        [ContextProperty("Ограничение", "Restriction")]
        public bool Restriction => Contains(XmlSchemaDerivationMethod.Restriction);

        [ContextProperty("Расширение", "Extension")]
        public bool Extension => Contains(XmlSchemaDerivationMethod.Extension);

        [ContextProperty("Список", "List")]
        public bool List => Contains(XmlSchemaDerivationMethod.List);

        #endregion

        #region Methods

        [ContextMethod("Значения", "Values")]
        public ArrayImpl Values() => _values;

        #endregion

        #endregion
    }
}