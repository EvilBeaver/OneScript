/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using OneScript.Exceptions;
using OneScript.StandardLibrary.Collections;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Machine;

namespace OneScript.StandardLibrary.TypeDescriptions
{
    internal class TypeList
    {
        private readonly IList<BslTypeValue> _list;

        public TypeList(IEnumerable<BslTypeValue> list)
        {
            _list = new List<BslTypeValue>(list);
        }

        public int Count => _list.Count; 

        public bool Contains(BslTypeValue type)
        {
            return _list.Contains(type);
        }

        public bool Contains(string typeName)
        {
            return _list.Any(x => string.Compare(
                x.TypeValue.Name, 
                typeName, 
                StringComparison.OrdinalIgnoreCase) == 0
            );
        }

        public IList<BslTypeValue> List()
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

        public static TypeList Construct(ITypeManager typeManager, IValue types, int nParam)
        {
            types = types?.GetRawValue();
            if (types == null || types.SystemType == BasicTypes.Undefined)
                return new TypeList(Array.Empty<BslTypeValue>());
			
            if (types.SystemType == BasicTypes.String)
            {
                return FromTypeNames(typeManager, types.AsString());
            }
            if (types is ArrayImpl arrayOfTypes)
            {
                return FromArrayOfTypes(arrayOfTypes);
            } 
			
            throw RuntimeException.InvalidNthArgumentType(nParam);
        }

        public static TypeList FromTypeNames(ITypeManager typeManager, string types)
        {
            var typeNames = types.Split(',');
            var typesList = new List<BslTypeValue>();
            foreach (var typeName in typeNames)
            {
                if (string.IsNullOrWhiteSpace(typeName))
                    continue;

                var typeValue = new BslTypeValue(typeManager.GetTypeByName(typeName.Trim()));
                if (!typesList.Contains(typeValue))
                    typesList.Add(typeValue);
            }

            return new TypeList(typesList);
        }

        public static TypeList FromArrayOfTypes(ArrayImpl arrayOfTypes)
        {
            var typesList = new List<BslTypeValue>();
            foreach (var type in arrayOfTypes)
            {
                var rawType = type.GetRawValue() as BslTypeValue;
                if (rawType == null)
                    continue;

                if (!typesList.Contains(rawType))
                    typesList.Add(rawType);
            }
            return new TypeList(typesList);
        }
    }
}