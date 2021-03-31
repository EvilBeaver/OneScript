/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Commons;
using OneScript.Core;
using ScriptEngine.Types;

namespace ScriptEngine.Machine.Values
{
    public class BooleanValue : GenericValue
    {
        public static readonly BooleanValue True = new BooleanValue(true);
        public static readonly BooleanValue False = new BooleanValue(false);

        private static string _trueString;
        private static string _falseString;

        private bool _value = false;

        private BooleanValue(bool value)
        {
            _value = value;
        }

        static BooleanValue()
        {
            RefreshLocalizedStrings();
            Locale.SystemLocaleChanged += RefreshLocalizedStrings;
        }

        private static void RefreshLocalizedStrings()
        {
            _trueString = Locale.NStr("ru = 'Да'; en = 'True'");
            _falseString = Locale.NStr("ru = 'Нет'; en = 'False'");
        }

        public override bool AsBoolean()
        {
            return _value;
        }

        public override TypeDescriptor SystemType => BasicTypes.Boolean;

        public override decimal AsNumber()
        {
            return _value ? 1 : 0;
        }

        public override string AsString()
        {
            return _value ? _trueString : _falseString;
        }

        public override int CompareTo(IValue other)
        {
            if (other.SystemType.Equals(BasicTypes.Number) || other.SystemType.Equals(BasicTypes.Boolean))
            {
                return AsNumber().CompareTo(other.AsNumber());
            }

            return base.CompareTo(other);
        }
    }
}