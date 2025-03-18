/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using OneScript.Types;

namespace ScriptEngine.Machine
{
    public interface IValue : IComparable<IValue>, IEquatable<IValue>
    {
        //DataType DataType { get; }
        TypeDescriptor SystemType { get; }

        // TODO: Избавиться нахер от этого ужоса
        IValue GetRawValue();
    }

    public static class IValueExtension
    {
        public static bool StrictEquals(this IValue value, IValue other)
            => value.Equals(other) && value.SystemType == other.SystemType;
    }
}