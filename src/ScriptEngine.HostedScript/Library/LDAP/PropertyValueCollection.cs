using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.DirectoryServices;

namespace ScriptEngine.HostedScript.Library.LDAP
{
    [ContextClass("КоллекцияЗначенийСвойства", "PropertyValueCollection")]
    class PropertyValueCollectionImpl: AutoContext<PropertyValueCollectionImpl>, ICollectionContext, IEnumerable<IValue>
    {
        private readonly PropertyValueCollection _values;

        [ContextMethod("Вместимость", "Capacity")]
        public int Capacity() => _values.Capacity;

        [ContextMethod("ЗначениеСтрокой", "ValueAsString")]
        public IValue GetValue()
        { 
            return ValueFactory.Create(_values.Value.ToString());
        }

        public PropertyValueCollectionImpl(PropertyValueCollection values)
        {
            _values = values;
        }
 
        [ScriptConstructor]
        public static PropertyCollectionImpl Constructor(PropertyCollection values)
        {
            return new PropertyCollectionImpl(values);
        }

        public override bool IsIndexed
        {
            get
            {
                return true;
            }
        }

        public override bool IsPropReadable(int propNum)
        {
            throw new NotImplementedException();
        }

        public override bool IsPropWritable(int propNum)
        {
            throw new NotImplementedException();
        }

        #region ICollectionContext Members

        [ContextMethod("Количество", "Count")]
        public int Count()
        {
            return _values.Count;
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(GetEnumerator());
        }

        #endregion

        #region IEnumerable<IRuntimeContextInstance> Members

        public IEnumerator<IValue> GetEnumerator()
        {
            foreach (var item in _values)
            {
                yield return ValueFactory.Create(item.ToString());
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        [ContextMethod("Получить", "Get")]
        public IValue Get(int index)
        {
            return ValueFactory.Create(_values[index].ToString());
        }

        public override IValue GetIndexedValue(IValue index)
        {
            if (index.DataType == DataType.Number)
                return Get((int)index.AsNumber());

            return base.GetIndexedValue(index);
        }

    }
}
