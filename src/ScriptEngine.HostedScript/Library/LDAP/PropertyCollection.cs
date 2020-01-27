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
    [ContextClass("КоллекцияСвойствLDAP", "LDAPPropertyCollection")]
    class LDAPPropertyCollectionImpl : AutoContext<LDAPPropertyCollectionImpl>, ICollectionContext, IEnumerable<IValue>
    {
        private readonly PropertyCollection _values;

        public LDAPPropertyCollectionImpl(PropertyCollection values)
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

        public override int GetPropCount() => _values.Count;

        public override string GetPropName(int propNum)
        {
            return _values.PropertyNames.Cast<string>().ToArray()[propNum];
        }

        [ContextMethod("ИменаСвойств", "PropertyNames")]
        public ArrayImpl PropertyNames() 
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
                yield return new LDAPPropertyValueCollectionImpl(item);
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
            return new LDAPPropertyValueCollectionImpl(_values[index]);
        }

        [ContextMethod("Содержит", "Contains")]
        public bool Contains(string propertyName)
        {
            return _values.Contains(propertyName);
        }

        [ScriptConstructor]
        public static LDAPPropertyCollectionImpl Constructor(PropertyCollection values)
        {
            return new LDAPPropertyCollectionImpl(values);
        }

    }
}
