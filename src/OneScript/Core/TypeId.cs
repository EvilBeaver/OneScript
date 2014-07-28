using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    public struct TypeId : IEquatable<TypeId>, IComparable<TypeId>
    {
        private Guid _value;

        public override bool Equals(object obj)
        {
            if(obj == null)
            {
                return false;
            }
            else if(obj.GetType() == typeof(TypeId))
            {
                var ti = (TypeId)obj;
                return this.Equals(ti);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public bool Equals(TypeId other)
        {
            return this._value == other._value;
        }

        public static TypeId New(string uuid)
        {
            return new TypeId() { _value = new Guid(uuid) };
        }

        public static TypeId New()
        {
            return new TypeId() { _value = Guid.NewGuid() };
        }

        public int CompareTo(TypeId other)
        {
            return _value.CompareTo(other._value);
        }
    }
}
