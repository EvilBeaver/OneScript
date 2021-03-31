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
    public class NullValue : GenericValue
    {
        public static readonly NullValue Instance;

        static NullValue()
        {
            Instance = new NullValue();
        }

        public NullValue()
        {
        }

        public override TypeDescriptor SystemType => BasicTypes.Null;

        public override string AsString()
        {
            return string.Empty;
        }
        
        public override bool IsEmpty => true;
    }
}
