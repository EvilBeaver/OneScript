/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Commons;
using OneScript.Exceptions;
using OneScript.Localization;
using OneScript.Types;
using System;
using ScriptEngine.Machine;

namespace OneScript.Values
{
    public abstract class EnumerationValue : BslValue
    {
        readonly TypeDescriptor _systemType;
        readonly string _name, _alias;

        public EnumerationValue(TypeDescriptor systemType, string name, string alias)
        {
            if (!Utils.IsValidIdentifier(name))
                throw new ArgumentException("Name must be a valid identifier", "name");

            if(alias != null && !Utils.IsValidIdentifier(alias))
                throw new ArgumentException("Name must be a valid identifier", "alias");

            _systemType = systemType;
            _name = name;
            _alias = alias;
        }

        public string Name => _name;
        public string Alias => _alias;

        public bool IsFilled() => true;

        public override TypeDescriptor SystemType => _systemType;

        public override string ToString()
        {
            return BilingualString.Localize(_name, _alias);
        }

        public override IValue GetRawValue() => this;

        public override int CompareTo(BslValue other)
        {
            throw RuntimeException.ComparisonNotSupportedException();
        }

        public override bool Equals(BslValue other)
        {
            return ReferenceEquals(other?.GetRawValue(), this);
        }
    }
}
