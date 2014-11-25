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
        private static DataType _null;

        static BasicTypes()
        {
            _number = DataType.CreateType("Число", "Number", TypeId.New("A7403601-38B7-420A-8630-84A1F675E102"));
            _string = DataType.CreateType("Строка", "String", TypeId.New("1AF7A333-33A1-45E2-BC5E-65CE2455D619"));
            _date = DataType.CreateType("Дата", "Date", TypeId.New("A7E20D06-E876-4949-9DB2-7C93891F3B87"));
            _boolean = DataType.CreateType("Булево", "Boolean", TypeId.New("34EDD2E3-DD29-4829-A424-67B3404AF423"));
            _undefined = DataType.CreateType("Неопределено", "Undefined", TypeId.New("783CE532-8CE0-4C59-BEF4-835AEFB715E4"));
            _type = DataType.CreateType("Тип", "Type", TypeId.New("4BCF3B78-B525-4159-8180-C76EDA613652"));
            _null = DataType.CreateType("Null", "Null", TypeId.New("26D78088-915A-4294-97E1-FB39E70187A6"));
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
        
        public static DataType Null
        {
            get { return _null; }
        }

    }
}
