/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using OneScript.Contexts;
using OneScript.Execution;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Collections
{
    /// <summary>
    /// Класс Соответствие, который оборачивает произвольный Dictionary
    /// </summary>
    public class MapWrapper<TKey, TValue> : AutoCollectionContext<MapImpl, KeyAndValueImpl>
    {
        private readonly IDictionary<TKey, TValue> _originalMap;

        private static ContextMethodsMapper<MapWrapper<TKey, TValue>> _methods =
            new ContextMethodsMapper<MapWrapper<TKey, TValue>>();

        public static MapWrapper<TKey, TValue> Create(ITypeManager typeManager, IDictionary<TKey, TValue> originalMap)
        {
            var type = typeManager.GetTypeByFrameworkType(typeof(MapImpl));
            return new MapWrapper<TKey, TValue>(type, originalMap);
        }
        
        private MapWrapper(
            TypeDescriptor mapType,
            IDictionary<TKey, TValue> originalMap) : base(mapType)
        {
            _originalMap = originalMap;
        }

        public override bool IsIndexed => true;

        [ContextMethod("Количество", "Count")]
        public override int Count() => _originalMap.Count;

        public override IEnumerator<KeyAndValueImpl> GetEnumerator()
        {
            return _originalMap.Select(entry => new KeyAndValueImpl(
                ContextValuesMarshaller.ConvertReturnValue(entry.Key),
                ContextValuesMarshaller.ConvertReturnValue(entry.Value))).GetEnumerator();
        }
        
        public override IValue GetIndexedValue(IValue index)
        {
            if (!_originalMap.TryGetValue(ContextValuesMarshaller.ConvertParam<TKey>(index), out var mapValue))
            {
                return ValueFactory.Create();
            }

            return ContextValuesMarshaller.ConvertReturnValue(mapValue);
        }

        public override void SetIndexedValue(IValue index, IValue val)
        {
            if (index.SystemType != BasicTypes.Undefined)
            {
                var mapKey = ContextValuesMarshaller.ConvertParam<TKey>(index);
                var mapVal = ContextValuesMarshaller.ConvertParam<TValue>(val);
                _originalMap[mapKey] = mapVal;
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
        
        [ContextMethod("Вставить", "Insert")]
        public void Insert(IValue key, IValue val = null)
        {
            SetIndexedValue(key, val ?? ValueFactory.Create());
        }

        [ContextMethod("Получить", "Get")]
        public IValue Retrieve(IValue key)
        {
            return GetIndexedValue(key);
        }

        [ContextMethod("Очистить", "Clear")]
        public void Clear()
        {
            _originalMap.Clear();
        }

        [ContextMethod("Удалить", "Delete")]
        public void Delete(IValue key)
        {
            _originalMap.Remove(ContextValuesMarshaller.ConvertParam<TKey>(key));
        }

        public override int GetMethodNumber(string name) => _methods.FindMethod(name);

        public override int GetMethodsCount() => _methods.Count;

        public override BslMethodInfo GetMethodInfo(int methodNumber) => _methods.GetRuntimeMethod(methodNumber);

        public override void CallAsProcedure(int methodNumber, IValue[] arguments, IBslProcess process)
        {
            var binding = _methods.GetCallableDelegate(methodNumber);
            try
            {
                binding(this, arguments, process);
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                throw e.InnerException!;
            }
        }
        
        public override void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue, IBslProcess process)
        {
            var binding = _methods.GetCallableDelegate(methodNumber);
            try
            {
                retValue = binding(this, arguments, process);
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                throw e.InnerException!;
            }
        }
    }
}