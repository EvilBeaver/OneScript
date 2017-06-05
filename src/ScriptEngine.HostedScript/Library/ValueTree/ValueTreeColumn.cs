/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript.Library.ValueTree
{
    /// <summary>
    /// Колонка дерева значений.
    /// </summary>
    [ContextClass("КолонкаДереваЗначений", "ValueTreeColumn")]
    public class ValueTreeColumn : AutoContext<ValueTreeColumn>
    {
        private string _title;
        private string _name;
        private TypeDescription _valueType;
        private int _width;
        private readonly ValueTreeColumnCollection _owner;

        // id нужен для правильной работы функции FindProperty.
        // Порядковый номер колонки не может быть использовать из-за своей изменчивости.
        private readonly int _id;

        public ValueTreeColumn(ValueTreeColumnCollection Owner, int id, string Name, string Title, TypeDescription Type, int Width)
        {
            _name = Name;
            _title = Title ?? Name;
            _valueType = Type ?? new TypeDescription();
            _width = Width;

            _owner = Owner;
            _id = id;

        }

        public ValueTreeColumn(ValueTreeColumnCollection Owner, int id, ValueTreeColumn src)
        {
            _name = src._name;
            _title = src._title;
            _valueType = src._valueType;
            _width = src._width;

            _owner = Owner;
            _id = id;
        }

        public int ID
        {
            get { return _id; }
        }

        [ContextProperty("Заголовок", "Title")]
        public string Title
        {
            get { return _title ?? _name; }
            set { _title = value; }
        }

        [ContextProperty("Имя", "Name")]
        public string Name
        {
            get { return _name; }
            set
            {
                if (_owner.FindColumnByName(value) != null)
                    throw new RuntimeException("Неверное имя колонки!");

                if (_title == _name)
                    _title = value;

                _name = value;
            }
        }

        [ContextProperty("ТипЗначения", "ValueType")]
        public TypeDescription ValueType
        {
            get { return _valueType; }
        }

        [ContextProperty("Ширина", "Width")]
        public int Width
        {
            get { return _width; }
            set { _width = value; } // TOOD: Проверить неотрицательность значения
        }

    }
}
