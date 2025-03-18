/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using OneScript.Contexts;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.Collections.Generic;
using OneScript.Values;
using Microsoft.Extensions.Primitives;

namespace OneScript.Web.Server
{
    [ContextClass("СтроковыеЗначения", "StringValues")]
    public class StringValuesWrapper : AutoCollectionContext<StringValuesWrapper, IValue>
    {
        private readonly StringValues _value;

        public static implicit operator StringValues(StringValuesWrapper d) => d._value;
        public static implicit operator StringValuesWrapper(StringValues b) => new(b);

        internal StringValuesWrapper(StringValues value) 
        {
            _value = value;
        }

        public override bool IsIndexed => true;

        public override IValue GetIndexedValue(IValue index)
        {
            var value = (int)index.AsNumber();

            return ValueFactory.Create(_value[value]);
        }

        #region ICollectionContext Members

        [ContextMethod("Получить", "Get")]
        public IValue Retrieve(IValue key)
        {
            return GetIndexedValue(key);
        }

        [ContextMethod("Количество", "Count")]
        public override int Count()
        {
            return _value.Count;
        }

        #endregion

        #region IEnumerable<IValue> Members

        public override IEnumerator<IValue> GetEnumerator()
        {
            foreach (var item in _value)
                yield return BslStringValue.Create(item);
        }

        #endregion

        protected override string ConvertToString()
        {
            return _value.ToString();
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }
    }
}
