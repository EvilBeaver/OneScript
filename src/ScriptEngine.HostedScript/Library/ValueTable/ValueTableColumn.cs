/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine;


namespace ScriptEngine.HostedScript.Library.ValueTable
{
    /// <summary>
    /// Колонка таблицы значений. 
    /// </summary>
    [ContextClass("КолонкаТаблицыЗначений", "ValueTableColumn")]
    public class ValueTableColumn : AutoContext<ValueTableColumn>
    {
        private string _title;
        private string _name;
        private TypeDescription _valueType;
        private int _width;
        private readonly WeakReference _owner;

        // id нужен для правильной работы функции FindProperty.
        // Порядковый номер колонки не может быть использовать из-за своей изменчивости.
        private readonly int _id;

        public ValueTableColumn(ValueTableColumnCollection Owner, int id, string Name, string Title, TypeDescription Type, int Width)
        {
            _name = Name;
            _title = Title;
            _valueType = Type ?? new TypeDescription();
            _width = Width;

            _owner = new WeakReference(Owner);
            _id = id;

        }

        public int ID
        {
            get { return _id; }
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
                    throw new RuntimeException("Неверное имя колонки!");

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
        public TypeDescription ValueType
        {
            get { return _valueType; }
        }

        /// <summary>
        /// Ширина колонки
        /// </summary>
        /// <value>Число</value>
        [ContextProperty("Ширина", "Width")]
        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }
    }
}
