/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Xml.Schema;
using OneScript.Core;
using OneScript.StandardLibrary.Collections;
using OneScript.StandardLibrary.XMLSchema.Enumerations;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.XMLSchema.Collections
{
    [ContextClass("ОбъединениеЗавершенностиПростогоТипаXS", "XSSimpleFinalUnion")]
    public class XSSimpleFinalUnion : AutoContext<XSSimpleFinalUnion>
    {
        private ArrayImpl _values;

        private bool Contains(XmlSchemaDerivationMethod _value)
        {
            XSSimpleFinal _enumValue = EnumerationXSSimpleFinal.FromNativeValue(_value);
            IValue _idx = _values.Find(_enumValue);
            return (_idx.DataType != DataType.Undefined);
        }

        public XSSimpleFinalUnion() => _values = ArrayImpl.Constructor();
       
        #region OneScript

        #region Properties

        [ContextProperty("Все", "All")]
        public bool All => Contains(XmlSchemaDerivationMethod.All);

        [ContextProperty("Объединение", "Union")]
        public bool Union => Contains(XmlSchemaDerivationMethod.Union);

        [ContextProperty("Ограничение", "Restriction")]
        public bool Restriction => Contains(XmlSchemaDerivationMethod.Restriction);

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
