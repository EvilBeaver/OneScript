/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Core;
using ScriptEngine.Types;

namespace ScriptEngine.Machine.Values
{
    public class UndefinedValue : GenericValue
    {
        public static UndefinedValue Instance { get; } = new UndefinedValue();


        public override int CompareTo(IValue other)
        {
            if(other.SystemType == SystemType)
                return 0;

            return base.CompareTo(other);
        }

        public override TypeDescriptor SystemType => BasicTypes.Undefined;

        public override string AsString()
        {
            return string.Empty;
        }

        public override bool IsEmpty => true;
    }
}