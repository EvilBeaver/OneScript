using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    public delegate IValue DataTypeConstructor(DataType constructedType, IValue[] arguments);

    public class DataType : IComparable<DataType>
    {
        private DataTypeConstructor _constructionDelegate;

        private DataType()
        {
        }

        public string Name { get; private set; }
        public string Alias { get; private set; }
        public bool IsObject { get; private set; }

        public IValue CreateInstance(IValue[] arguments)
        {
            if(_constructionDelegate != null)
            {
                return _constructionDelegate(this, arguments);
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

        public int CompareTo(DataType other)
        {
            return string.Compare(this.Name, other.Name, true);
        }

        internal static DataType CreateSimpleType(string name, string alias = null, DataTypeConstructor constructor = null)
        {
            return new DataType()
            {
                Name = name,
                Alias = alias,
                IsObject = false,
                _constructionDelegate = constructor
            };
        }

        internal static DataType CreateObjectType(string name, string alias = null, DataTypeConstructor constructor = null)
        {
            return new DataType()
            {
                Name = name,
                Alias = alias,
                IsObject = true,
                _constructionDelegate = constructor
            };
        }

    }
}
