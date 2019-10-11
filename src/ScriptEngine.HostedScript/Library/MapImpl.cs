/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System.Collections.Generic;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library
{
    [ContextClass("Соответствие", "Map")]
    public class MapImpl : AutoContext<MapImpl>, ICollectionContext, IEnumerable<KeyAndValueImpl>
    {
        private readonly Dictionary<IValue, IValue> _content = new Dictionary<IValue, IValue>(new GenericIValueComparer());

        public MapImpl()
        {
        }

        public MapImpl(IEnumerable<KeyAndValueImpl> source)
        {
            foreach (var kv in source)
            {
                _content.Add(kv.Key.GetRawValue(), kv.Value.GetRawValue());
            }
        }
        
        public override bool IsIndexed
        {
            get
            {
                return true;
            }
        }

        public override IValue GetIndexedValue(IValue index)
        {
            IValue result;
            if (!_content.TryGetValue(index, out result))
            {
                result = ValueFactory.Create();
            }

            return result;
        }

        public override void SetIndexedValue(IValue index, IValue val)
        {
            if (index.DataType != DataType.Undefined)
            {
                _content[index] = val;
            }
        }

        public override bool IsPropReadable(int propNum)
        {
            return false;
        }

        public override bool IsPropWritable(int propNum)
        {
            return false;
        }
        
        internal bool ContainsKey(IValue key)
        {
            return _content.ContainsKey(key);
        }

        #region ICollectionContext Members

        [ContextMethod("Вставить", "Insert")]
        public void Insert(IValue key, IValue val=null)
        {
            SetIndexedValue(key, val ?? ValueFactory.Create() );
        }

        [ContextMethod("Получить", "Get")]
        public IValue Retrieve(IValue key)
        {
            return GetIndexedValue(key);
        }

        [ContextMethod("Количество", "Count")]
        public int Count()
        {
            return _content.Count;
        }

        [ContextMethod("Очистить", "Clear")]
        public void Clear()
        {
            _content.Clear();
        }

        [ContextMethod("Удалить", "Delete")]
        public void Delete(IValue key)
        {
            _content.Remove(key);
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(GetEnumerator());
        }

        #endregion

        #region IEnumerable<IValue> Members

        public IEnumerator<KeyAndValueImpl> GetEnumerator()
        {
            foreach (var item in _content)
            {
                yield return new KeyAndValueImpl(item.Key, item.Value);
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        [ScriptConstructor]
        public static MapImpl Constructor()
        {
            return new MapImpl();
        }
        
        [ScriptConstructor(Name = "Из фиксированного соответствия")]
        public static MapImpl Constructor(IValue source)
        {
            if (!(source.GetRawValue() is FixedMapImpl fix))
                throw RuntimeException.InvalidArgumentType();
            
            return new MapImpl(fix);
        }
    }
}
