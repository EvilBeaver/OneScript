/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Values;

namespace ScriptEngine.HostedScript.Library
{
    internal class TypeList
    {
        private readonly IList<TypeTypeValue> _list;

        public TypeList(IEnumerable<TypeTypeValue> list)
        {
            _list = new List<TypeTypeValue>(list);
        }

        public int Count => _list.Count; 

        public bool Contains(TypeTypeValue type)
        {
            return _list.Contains(type);
        }

        public bool Contains(string typeName)
        {
            return _list.Any(x => string.Compare(
                x.Value.Name, 
                typeName, 
                StringComparison.OrdinalIgnoreCase) == 0
            );
        }

        public IList<TypeTypeValue> List()
        {
            return _list;
        }

        /// <summary>
        /// Возвращает типы как Массив
        /// </summary>
        /// <returns>Массив, содержащий элементы типа Тип</returns>
        public ArrayImpl AsArray()
        {
            return new ArrayImpl(_list);
        }

        public TypeList Modify(TypeList typesToRemove, TypeList typesToAdd)
        {
            var typesList = _list.Where(type => !typesToRemove.Contains(type)).ToList();
            typesList.AddRange(typesToAdd._list);
            return new TypeList(typesList);
        }

        public TypeList Modify(TypeList typesToRemove)
        {
            var typesList = _list.Where(type => !typesToRemove.Contains(type)).ToList();
            return new TypeList(typesList);
        }

        public static TypeList Construct(IValue types)
        {
            types = types?.GetRawValue();
            if (types == null || types.DataType == DataType.Undefined)
                return new TypeList(Array.Empty<TypeTypeValue>());
			
            if (types.DataType == DataType.String)
            {
                return FromTypeNames(types.AsString());
            }
            if (types is ArrayImpl arrayOfTypes)
            {
                return FromArrayOfTypes(arrayOfTypes);
            }

            return null;
        }

        public static TypeList FromTypeNames(string types)
        {
            var typeNames = types.Split(',');
            var typesList = new List<TypeTypeValue>();
            foreach (var typeName in typeNames)
            {
                if (string.IsNullOrWhiteSpace(typeName))
                    continue;

                var typeValue = new TypeTypeValue(typeName.Trim());
                if (!typesList.Contains(typeValue))
                    typesList.Add(typeValue);
            }

            return new TypeList(typesList);
        }

        public static TypeList FromArrayOfTypes(ArrayImpl arrayOfTypes)
        {
            var typesList = new List<TypeTypeValue>();
            foreach (var type in arrayOfTypes)
            {
                var rawType = type.GetRawValue() as TypeTypeValue;
                if (rawType == null)
                    continue;

                if (!typesList.Contains(rawType))
                    typesList.Add(rawType);
            }
            return new TypeList(typesList);
        }
    }
}