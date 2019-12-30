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
    [ContextClass("КоллекцияСвойств", "PropertyCollection")]
    class PropertyCollectionImpl : AutoContext<PropertyCollectionImpl>, ICollectionContext, IEnumerable<IValue>
    {
        private readonly PropertyCollection _values;

        public PropertyCollectionImpl(PropertyCollection values)
        {
            _values = values;
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
        public override IValue GetIndexedValue(IValue index)
        {
            return Get(index.AsString());
        }

        public override int GetPropCount()
        {
            return _values.Count;
        }

        public override string GetPropName(int propNum)
        {
            return _values.PropertyNames.Cast<string>().ToArray()[propNum];
        }

        [ContextProperty("ИменаСвойств", "PropertyNames")]
        public ArrayImpl PropertyNames => GetPropertyNames();

        private ArrayImpl GetPropertyNames()
        {
            return new ArrayImpl(_values.PropertyNames.Cast<string>().Select(i => ValueFactory.Create(i)).ToArray());
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
            foreach (PropertyValueCollection item in _values)
            {
                yield return new PropertyValueCollectionImpl(item);
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
        public IValue Get(string index)
        {
            return new PropertyValueCollectionImpl(_values[index]);
        }

        [ContextMethod("Содержит", "Contains")]
        public bool Contains(string propertyName)
        {
            return _values.Contains(propertyName);
        }

        [ScriptConstructor]
        public static PropertyCollectionImpl Constructor(PropertyCollection values)
        {
            return new PropertyCollectionImpl(values);
        }

    }
}
