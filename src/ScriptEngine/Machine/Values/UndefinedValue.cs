/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace ScriptEngine.Machine.Values
{
    public class UndefinedValue : GenericValue
    {
        public static UndefinedValue Instance { get; } = new UndefinedValue();

        private UndefinedValue()
        {
            DataType = DataType.Undefined;
        }

        public override string AsString()
        {
            return string.Empty;
        }
    }
}