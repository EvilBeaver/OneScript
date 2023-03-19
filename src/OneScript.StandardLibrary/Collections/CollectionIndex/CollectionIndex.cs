/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Localization;
using OneScript.StandardLibrary.Collections.ValueTable;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;

namespace OneScript.StandardLibrary.Collections.CollectionIndex
{
    [ContextClass("ИндексКоллекции", "CollectionIndex", TypeUUID = "48D150D4-A0DA-47CA-AEA3-D4078A731C11")]
    public class CollectionIndex : AutoCollectionContext<CollectionIndex, IValue>
    {
        private static readonly TypeDescriptor _objectType = typeof(ValueTableColumnCollection).GetTypeFromClassMarkup();

        private readonly IIndexSourceCollection _source;
        private readonly List<IValue> _fields;

        public CollectionIndex(IIndexSourceCollection source, IEnumerable<IValue> fields) : base(_objectType)
        {
            _source = source;
            _fields = new List<IValue>(fields);
        }

        internal void ColumnRemoved(IValue column)
        {
            if (_fields.Contains(column))
            {
                _fields.RemoveAll((_c => _c.Equals(column)));
                Rebuild();
            }
        }

        public void AddElement(PropertyNameIndexAccessor element)
        {
            
        }

        private void Rebuild()
        {
            
        }

        internal void Clear()
        {
            
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            var empty = true;
            foreach (var field in _fields)
            {
                if (!empty)
                    builder.Append(", ");
                builder.Append(_source.GetName(field));
                empty = false;
            }

            return builder.ToString();
        }

        public override bool IsIndexed => true;

        public string GetIndexedValue(int index)
        {
            if (index >= 0 && index < _fields.Count)
            {
                return _source.GetName(_fields[index]);
            }
            throw RuntimeException.InvalidArgumentValue();
        }

        public override IValue GetIndexedValue(IValue index)
        {
            var rawIndex = index.GetRawValue();
            if (rawIndex.SystemType == BasicTypes.Number)
            {
                var intIndex = decimal.ToInt32(rawIndex.AsNumber());
                return ValueFactory.Create(GetIndexedValue(intIndex));
            }

            throw RuntimeException.InvalidArgumentType(nameof(index));
        }

        public void SetIndexedValue(int index, string value)
        {
            throw new RuntimeException(new BilingualString("Элемент доступен только для чтения"));
        }

        public override int Count()
        {
            return _fields.Count;
        }

        public override IEnumerator<IValue> GetEnumerator()
        {
            foreach (var field in _fields)
            {
                yield return ValueFactory.Create(_source.GetName(field));
            }
        }
    }
}
