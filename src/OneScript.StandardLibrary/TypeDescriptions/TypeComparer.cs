/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using OneScript.Types;
using OneScript.Values;

namespace OneScript.StandardLibrary.TypeDescriptions
{
    internal class TypeComparer : IComparer<BslTypeValue>
    {
        private const string TYPE_BINARYDATA_NAME = "ДвоичныеДанные";
        private static readonly IDictionary<TypeDescriptor, int> primitives = new Dictionary<TypeDescriptor, int>();

        public int Compare(BslTypeValue x, BslTypeValue y)
        {
            if (x.TypeValue.Equals(y)) return 0;

            var primitiveX = PrimitiveIndex(x);
            var primitiveY = PrimitiveIndex(y);

            if (primitiveX != -1)
            {
                if (primitiveY != -1)
                    return primitiveX - primitiveY;

                return -1;
            }

            if (primitiveY != -1)
                return 1;

            return x.TypeValue.Id.CompareTo(y.TypeValue.Id);
        }

        private int PrimitiveIndex(BslTypeValue type)
        {
            if (StringComparer.CurrentCultureIgnoreCase.Equals(type.TypeValue.Name, TYPE_BINARYDATA_NAME))
            {
                // Пора двоичным данным стать примитивом
                return 1;
            }

            if (primitives.TryGetValue(type.TypeValue, out var index))
                return index;

            return -1;
        }

        static TypeComparer()
        {
            primitives.Add(BasicTypes.Boolean, 0);
            primitives.Add(BasicTypes.String, 2);
            primitives.Add(BasicTypes.Date, 3);
            primitives.Add(BasicTypes.Null, 4);
            primitives.Add(BasicTypes.Number, 5);
            primitives.Add(BasicTypes.Type, 6);
        }

    }
}
 