/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts;
using OneScript.StandardLibrary.Collections.Exceptions;
using OneScript.StandardLibrary.TypeDescriptions;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Collections.ValueTable
{
    /// <summary>
    /// Колонка таблицы значений. 
    /// </summary>
    [ContextClass("КолонкаТаблицыЗначений", "ValueTableColumn")]
    public class ValueTableColumn : AutoContext<ValueTableColumn>
    {
        private string _title;
        private string _name;
        private readonly TypeDescription _valueType;
        private readonly WeakReference _owner;

        private int _indicesCount = 0;
        public bool IsIndexable => _indicesCount != 0;
        public void AddToIndex() { _indicesCount++; }
        public void DeleteFromIndex() { if (_indicesCount != 0) _indicesCount--; }

        public ValueTableColumn(ValueTableColumnCollection owner, string name, string title, TypeDescription type, int width)
        {
            _name = name;
            _title = title;
            _valueType = type ?? new TypeDescription();
            Width = width;

            _owner = new WeakReference(owner);
        }

        /// <summary>
        /// Заголовок колонки
        /// </summary>
        /// <value>Строка</value>
        [ContextProperty("Заголовок", "Title")]
        public string Title
        {
            get { return _title ?? _name; }
            set { _title = value; }
        }

        /// <summary>
        /// Имя колонки
        /// </summary>
        /// <value>Строка</value>
        [ContextProperty("Имя", "Name")]
        public string Name
        {
            get { return _name; }
            set
            {
                ValueTableColumnCollection Owner = _owner.Target as ValueTableColumnCollection;
                if (Owner.FindColumnByName(value) != null)
                    throw ColumnException.WrongColumnName();

                if (_title == _name)
                    _title = value;

                _name = value;
            }
        }
        /// <summary>
        /// Тип значения колонки
        /// </summary>
        /// <value>ОписаниеТипа</value>
        [ContextProperty("ТипЗначения", "ValueType")]
        public TypeDescription ValueType => _valueType;

        /// <summary>
        /// Ширина колонки
        /// </summary>
        /// <value>Число</value>
        [ContextProperty("Ширина", "Width")]
        public int Width { get; set; }
    }
}
