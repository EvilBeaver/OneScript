/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Diagnostics;
using OneScript.Commons;
using OneScript.Core;

namespace OneScript.Core
{
    public sealed class TypeDescriptor : IEquatable<TypeDescriptor>
    {
        public TypeDescriptor(Guid id, string typeName, string alias = default, Type implementingClass = default)
        {
            Id = id;
            Name = typeName;
            Alias = alias;
            ImplementingClass = implementingClass ?? typeof(IValue);
        }

        public string Name { get; }
        
        public string Alias { get; }

        public Guid Id { get; }
        
        public Type ImplementingClass { get; }

        public override string ToString()
        {
            return Locale.UseAliasedPresentations? Alias ?? Name : Name;
        }

        public bool Equals(TypeDescriptor other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is TypeDescriptor other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(TypeDescriptor left, TypeDescriptor right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TypeDescriptor left, TypeDescriptor right)
        {
            return !Equals(left, right);
        }
    }
}