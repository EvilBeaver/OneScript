/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Commons;
using OneScript.StandardLibrary.TypeDescriptions;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;

namespace OneScript.StandardLibrary.Collections.ValueTable
{
    /// <summary>
    /// Колонка таблицы значений. 
    /// </summary>
    [ContextClass("КолонкаТаблицыЗначений", "ValueTableColumn", TypeUUID = "5B2F5C0D-8E08-4D01-A85F-9E24FCC92685")]
    public class ValueTableColumn : AutoContext<ValueTableColumn>
    {
        private string _title;
        private string _name;
        private TypeDescription _valueType;
        private int _width;
        private readonly WeakReference _owner;
        
        private static TypeDescriptor _instanceType = typeof(ValueTableColumn).GetTypeFromClassMarkup();

        public ValueTableColumn(ValueTableColumnCollection owner, string name, string title, TypeDescription type, int width)
            : base(_instanceType)
        {
            _name = name;
            _title = title;
            _valueType = type ?? new TypeDescription();
            _width = width;

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
