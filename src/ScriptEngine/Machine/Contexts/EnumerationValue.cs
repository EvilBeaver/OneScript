/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Exceptions;
using OneScript.Types;
using OneScript.Values;

namespace ScriptEngine.Machine.Contexts
{
    public abstract class EnumerationValue : BslValue
    {
        readonly EnumerationContext _owner;

        public EnumerationValue(EnumerationContext owner)
        {
            _owner = owner;
        }

        public EnumerationContext Owner
        {
            get
            {
                return _owner;
            }
        }

        public string ValuePresentation
        {
            get;set;
        }

        public bool IsFilled() => true;

        public override TypeDescriptor SystemType => _owner.ValuesType;

        public override string ToString()
        {
            return ValuePresentation == null ? SystemType.Name : ValuePresentation;
        }

        public override IValue GetRawValue()
        {
            return this;
        }

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
