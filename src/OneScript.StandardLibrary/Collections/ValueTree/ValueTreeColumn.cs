/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Collections.ValueTree
{
    /// <summary>
    /// Колонка дерева значений.
    /// </summary>
    [ContextClass("КолонкаДереваЗначений", "ValueTreeColumn")]
    public class ValueTreeColumn : AutoContext<ValueTreeColumn>
    {
        private string _title;
        private string _name;
        private TypeDescription.TypeDescription _valueType;
        private int _width;
        private readonly ValueTreeColumnCollection _owner;

        public ValueTreeColumn(ValueTreeColumnCollection owner, string name, string title, TypeDescription type, int width)
        {
            _name = name;
            _title = title ?? name;
            _valueType = type ?? new TypeDescription();
            _width = width;

            _owner = owner;
        }

        public ValueTreeColumn(ValueTreeColumnCollection owner, ValueTreeColumn src)
        {
            _name = src._name;
            _title = src._title;
            _valueType = src._valueType;
            _width = src._width;

            _owner = owner;
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
        public TypeDescription.TypeDescription ValueType
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
