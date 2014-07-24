using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    public class DataType
    {
        private DataType()
        {
        }

        public string Name { get; private set; }
        public string Alias { get; private set; }
        public bool IsObject { get; private set; }

        internal static DataType CreateSimple(string name, string alias)
        {
            return new DataType()
            {
                Name = name,
                Alias = alias,
                IsObject = false
            };
        }

        internal static DataType CreateObject(string name, string alias)
        {
            return new DataType()
            {
                Name = name,
                Alias = alias,
                IsObject = true
            };
        }
    }
}
