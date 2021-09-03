﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Commons;
using OneScript.Contexts;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Collections
{
    [ContextClass("ФиксированныйМассив", "FixedArray")]
    public class FixedArrayImpl : AutoCollectionContext<FixedArrayImpl, IValue>
    {
        private readonly ArrayImpl _array;

        public FixedArrayImpl(ArrayImpl source)
        {
            _array = new ArrayImpl();
            foreach (var Value in source)
            {
                _array.Add(Value);
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
            return _array.GetIndexedValue(index);
        }

        public override void SetIndexedValue(IValue index, IValue val)
        {
            throw new RuntimeException("Индексированное значение доступно только для чтения");
        }

        #region ICollectionContext Members

        [ContextMethod("Количество", "Count")]
        public override int Count()
        {
            return _array.Count();
        }

        public override IEnumerator<IValue> GetEnumerator()
        {
            return _array.GetEnumerator();
        }

        #endregion


        [ContextMethod("Найти", "Find")]
        public IValue Find(IValue what)
        {
            return _array.Find(what);
        }

        [ContextMethod("ВГраница", "UBound")]
        public int UpperBound()
        {
            return _array.UpperBound();
        }

        [ContextMethod("Получить", "Get")]
        public IValue Get(int index)
        {
            return _array.Get(index);
        }

        [ScriptConstructor(Name = "На основании обычного массива")]
        public static FixedArrayImpl Constructor(IValue source)
        {
            var rawSource = source.GetRawValue() as ArrayImpl;
            if (rawSource == null)
                throw RuntimeException.InvalidArgumentType();

            return new FixedArrayImpl(rawSource);
        }
    }
}
