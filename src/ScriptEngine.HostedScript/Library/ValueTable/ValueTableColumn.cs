using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine;


namespace ScriptEngine.HostedScript.Library.ValueTable
{
    [ContextClass("КолонкаТаблицыЗначений", "ValueTableColumn")]
    class ValueTableColumn : AutoContext<ValueTableColumn>
    {
        private string _title;
        private string _name;
        private IValue _valueType;
        private int _width;
        private WeakReference _owner;

        // id нужен для правильной работы функции FindProperty.
        // Порядковый номер колонки не может быть использовать из-за своей изменчивости.
        private int _id;

        public ValueTableColumn(ValueTableColumnCollection Owner, int id, string Name, string Title, IValue Type, int Width)
        {
            _name = Name;
            _title = Title;
            _valueType = Type;
            _width = Width;

            _owner = new WeakReference(Owner);
            _id = id;

        }

        public int ID
        {
            get { return _id; }
        }

        [ContextProperty("Заголовок", "Title")]
        public string Title
        {
            get { return _title == null ? _name : _title; }
            set { _title = value; }
        }

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
        [ContextProperty("ТипЗначения", "ValueType")]
        public IValue ValueType
        {
            get { return _valueType; }
            set { _valueType = value; } // TODO: Проверить тип
        }
        [ContextProperty("Ширина", "Width")]
        public int Width
        {
            get { return Width; }
            set { _width = value; } // TOOD: Проверить неотрицательность значения
        }
    }
}
