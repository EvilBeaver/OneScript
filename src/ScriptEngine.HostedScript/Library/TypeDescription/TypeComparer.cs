/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System.Collections.Generic;
using ScriptEngine.HostedScript.Library.Binary;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Values;

namespace ScriptEngine.HostedScript.Library
{
    internal class TypeComparer : IComparer<TypeTypeValue>
    {
        public int Compare(TypeTypeValue x, TypeTypeValue y)
        {
            if (x == null)
            {
                return y == null ? 0 : -1;
            }

            if (y == null) return 1;

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

            return x.Value.ID.CompareTo(y.Value.ID);
        }

        private static int PrimitiveIndex(TypeTypeValue type)
        {
            var typeDescriptor = TypeManager.GetTypeDescriptorFor(type);
            for (var primitiveIndex = 0; primitiveIndex < CommonTypes.Primitives.Length; primitiveIndex++)
            {
                if (typeDescriptor.Equals(CommonTypes.Primitives[primitiveIndex]))
                {
                    return primitiveIndex;
                }
            }
                
            return -1;
        }
    }
}