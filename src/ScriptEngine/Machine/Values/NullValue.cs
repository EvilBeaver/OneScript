/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

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
            DataType = DataType.GenericValue;
        }

        public override TypeDescriptor SystemType => TypeManager.GetTypeByFrameworkType(typeof(NullValue));

        public override string AsString()
        {
            return string.Empty;
        }
    }
}
