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
        protected EnumerationValue(TypeDescriptor systemType, string name, string alias)
        {
            if (!Utils.IsValidIdentifier(name))
                throw new ArgumentException("Name must be a valid identifier", nameof(name));

            if(alias != null && !Utils.IsValidIdentifier(alias))
                throw new ArgumentException("Name must be a valid identifier", nameof(alias));

            SystemType = systemType;
            Name = name;
            Alias = alias;
        }

        public string Name { get; }

        public string Alias { get; }

        public override TypeDescriptor SystemType { get; }

        public override string ToString()
        {
            return BilingualString.Localize(Name, Alias);
        }

        public override IValue GetRawValue() => this;

        public override bool Equals(BslValue other)
        {
            return ReferenceEquals(other?.GetRawValue(), this);
        }
    }
}
