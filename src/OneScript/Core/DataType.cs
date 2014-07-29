using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    public delegate IValue DataTypeConstructor(DataType constructedType, IValue[] arguments);

    public class DataType : IEquatable<DataType>, IComparable<DataType>
    {
        private DataType(TypeId id)
        {
            ID = id;
        }

        public string Name { get; internal set; }
        public string Alias { get; internal set; }
        public bool IsObject { get; internal set; }
        public DataTypeConstructor Constructor { get; internal set; }
        public TypeId ID { get; private set; }

        public IValue CreateInstance(IValue[] arguments)
        {
            if (Constructor != null)
            {
                return Constructor(this, arguments);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public bool Equals(DataType other)
        {
            return string.Compare(this.Name, other.Name, true) == 0;
        }

        public int CompareTo(DataType other)
        {
            return string.Compare(this.Name, other.Name, true);
        }

        internal static DataType CreateType(string name, string alias)
        {
            var id = TypeId.New();
            var type = new DataType(id);
            type.Name = name;
            type.Alias = alias;

            return type;
        }
    }
}
