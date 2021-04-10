/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Collections.ValueTree
{
    /// <summary>
    /// Дерево значений.
    /// Древовидная структура с фунциональностью подобно таблице значений.
    /// </summary>
    [ContextClass("ДеревоЗначений", "ValueTree")]
    public class ValueTree : AutoContext<ValueTree>
    {
        private readonly ValueTreeColumnCollection _columns = new ValueTreeColumnCollection();
        private readonly ValueTreeRowCollection _rows;

        public ValueTree()
        {
            _rows = new ValueTreeRowCollection(this, null, 0);
        }

        [ContextProperty("Колонки", "Columns")]
        public ValueTreeColumnCollection Columns
        {
            get { return _columns; }
        }

        [ContextProperty("Строки", "Rows")]
        public ValueTreeRowCollection Rows
        {
            get { return _rows;  }
        }

        /// <summary>
        /// Создаёт копию дерева значений.
        /// </summary>
        /// <param name="rows">Массив. Строки для копирования. Если не указан, копируются все строки. Необязательный параметр.</param>
        /// <param name="columnNames">Строка. Список колонок через запятую, которые должны быть скопированы. Необязательный параметр.</param>
        /// <returns>ДеревоЗначений. Копия исходного дерева значений.</returns>
        [ContextMethod("Скопировать", "Copy")]
        public ValueTree Copy(IValue rows = null, string columnNames = null)
        {

            // TODO: отрабатывать параметр Rows
            // TODO: отрабатывать параметр ColumnNames

            ValueTree result = new ValueTree();
            result._columns.CopyFrom(_columns);
            result._rows.CopyFrom(_rows);
            return result;
        }


        [ScriptConstructor]
        public static ValueTree Constructor()
        {
            return new ValueTree();
        }
    }
}
