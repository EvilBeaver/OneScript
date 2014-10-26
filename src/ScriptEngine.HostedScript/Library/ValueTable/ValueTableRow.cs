using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript.Library.ValueTable
{
    [ContextClass("СтрокаТаблицыЗначений", "ValueTableRow")]
    class ValueTableRow : DynamicPropertiesAccessor, ICollectionContext
    {
        private Dictionary<IValue, IValue> _data = new Dictionary<IValue, IValue>();
        private WeakReference _owner;

        public ValueTableRow(ValueTable Owner)
        {
            _owner = new WeakReference(Owner);
        }

        public int Count()
        {
            var Owner = _owner.Target as ValueTable;
            return Owner.Columns.Count();
        }

        public IEnumerator<IValue> GetEnumerator()
        {
            var Owner = _owner.Target as ValueTable;
            foreach (var item in Owner.Columns)
            {
                IValue Value;
                if (!_data.TryGetValue(item, out Value))
                    Value = ValueFactory.Create(); // TODO: Определять пустое значение для типа колонки
                yield return item;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(GetEnumerator());
        }

        public override int FindProperty(string name)
        {
            ValueTable Owner = _owner.Target as ValueTable;
            ValueTableColumn C = Owner.Columns.FindColumnByName(name);
            
            if (C == null)
                throw RuntimeException.PropNotFoundException(name);

            return C.ID;
        }

        public override IValue GetPropValue(int propNum)
        {
            ValueTable Owner = _owner.Target as ValueTable;
            ValueTableColumn C = Owner.Columns.FindColumnById(propNum);
            
            IValue Value;
            if (_data.TryGetValue(C, out Value))
                return Value;

            return ValueFactory.Create(); // TODO: Определять пустое значение по типу колонки
        }

        public override void SetPropValue(int propNum, IValue newVal)
        {
            ValueTable Owner = _owner.Target as ValueTable;
            ValueTableColumn C = Owner.Columns.FindColumnById(propNum);
            _data[C] = newVal;
        }

        private ValueTableColumn GetColumnByIIndex(IValue index)
        {
            ValueTable Owner = _owner.Target as ValueTable;
            return Owner.Columns.GetColumnByIIndex(index);
        }

        public override IValue GetIndexedValue(IValue index)
        {
            ValueTableColumn C = GetColumnByIIndex(index);

            IValue Value;
            if (_data.TryGetValue(C, out Value))
                return Value;

            return ValueFactory.Create(); // TODO: Определять пустое значение по типу колонки
        }

        public override void SetIndexedValue(IValue index, IValue val)
        {
            _data[GetColumnByIIndex(index)] = val;
        }
    }
}
