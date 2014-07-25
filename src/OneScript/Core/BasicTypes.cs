using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    public static class BasicTypes
    {
        private static DataType _number;
        private static DataType _string;
        private static DataType _date;
        private static DataType _boolean;
        private static DataType _undefined;
        private static DataType _type;

        static BasicTypes()
        {
            _number = DataType.CreateSimple("Число", "Number");
            _string = DataType.CreateSimple("Строка", "String");
            _date = DataType.CreateSimple("Дата", "Date");
            _boolean = DataType.CreateSimple("Булево", "Boolean");
            _undefined = DataType.CreateSimple("Неопределено", "Undefined");
            _type = DataType.CreateSimple("Тип", "Type");
        }

        public static DataType Number
        {
            get { return _number; }
        }

        public static DataType String
        {
            get { return _string; }
        }
        public static DataType Date
        {
            get { return _date; }
        }

        public static DataType Boolean
        {
            get { return _boolean; }
        }
        public static DataType Undefined
        {
            get { return _undefined; }
        }
        public static DataType Type
        {
            get { return _type; }
        }

    }
}
