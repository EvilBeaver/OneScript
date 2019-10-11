﻿/*----------------------------------------------------------
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
    [ContextClass("ФиксированноеСоответствие", "FixedMap")]
    public class FixedMapImpl : AutoContext<FixedMapImpl>, ICollectionContext, IEnumerable<KeyAndValueImpl>
    {

        private readonly MapImpl _map;

        public FixedMapImpl(MapImpl source)
        {
            _map = new MapImpl();
            foreach (KeyAndValueImpl KV in source)
            {
                _map.Insert(KV.Key, KV.Value);
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
            if(_map.ContainsKey(index))
                return _map.GetIndexedValue(index);

            throw new RuntimeException("Значение, соответствующее ключу, не задано");
        }

        public override void SetIndexedValue(IValue index, IValue val)
        {
            throw new RuntimeException("Индексированное значение доступно только для чтения");
        }

        public override bool IsPropReadable(int propNum)
        {
            return _map.IsPropReadable(propNum);
        }

        public override bool IsPropWritable(int propNum)
        {
            return _map.IsPropWritable(propNum);
        }
        
        #region ICollectionContext Members

        [ContextMethod("Получить", "Get")]
        public IValue Retrieve(IValue key)
        {
            return _map.GetIndexedValue(key);
        }

        [ContextMethod("Количество", "Count")]
        public int Count()
        {
            return _map.Count();
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return _map.GetManagedIterator();
        }

        #endregion

        #region IEnumerable<IValue> Members

        public IEnumerator<KeyAndValueImpl> GetEnumerator()
        {
            return _map.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        [ScriptConstructor(Name = "Из соответствия")]
        public static FixedMapImpl Constructor(IValue source)
        {
            var rawSource = source.GetRawValue() as MapImpl;
            if (rawSource == null)
                throw RuntimeException.InvalidArgumentType();

            return new FixedMapImpl(rawSource);
        }
    }

}
