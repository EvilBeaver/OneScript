using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript;
using OneScript.Core;

namespace OneScript.Tests
{
    [TestClass]
    public class TypeSystem
    {
        [TestMethod]
        public void TypeID_equality()
        {
            var first = new TypeId("1CB882B3-648C-4161-B640-49439059EC4C");
            var second = new TypeId("1CB882B3-648C-4161-B640-49439059EC4C");
            Assert.AreNotSame(first, second);
            Assert.AreEqual(first, second);
            Assert.IsTrue(first.Equals(second));
            Assert.IsTrue(first.CompareTo(second) == 0);
        }

        [TestMethod]
        public void TypeID_Comparison()
        {
            var first = new TypeId("1CB882B3-648C-4161-B640-49439059EC4C");
            var second = new TypeId("9051962F-DE5C-4267-8F26-5761DC499806");
            Assert.IsTrue(first.CompareTo(second) < 0);
        }
        
        [TestMethod]
        public void BasicTypes_Are_Unique()
        {
            DataType type1 = BasicTypes.Number;
            DataType type2 = BasicTypes.Number;
            Assert.AreSame(type1, type2);

            type1 = BasicTypes.String;
            type2 = BasicTypes.String;
            Assert.AreSame(type1, type2);

            type1 = BasicTypes.Date;
            type2 = BasicTypes.Date;
            Assert.AreSame(type1, type2);

            type1 = BasicTypes.Boolean;
            type2 = BasicTypes.Boolean;
            Assert.AreSame(type1, type2);

            type1 = BasicTypes.Undefined;
            type2 = BasicTypes.Undefined;
            Assert.AreSame(type1, type2);

            type1 = BasicTypes.Null;
            type2 = BasicTypes.Null;
            Assert.AreSame(type1, type2);
        }

        [TestMethod]
        public void BasicTypes_Names()
        {
            Assert.AreEqual("Число", BasicTypes.Number.Name);
            Assert.AreEqual("Строка", BasicTypes.String.Name);
            Assert.AreEqual("Дата", BasicTypes.Date.Name);
            Assert.AreEqual("Булево", BasicTypes.Boolean.Name);
            Assert.AreEqual("Неопределено", BasicTypes.Undefined.Name);
            Assert.AreEqual("Тип", BasicTypes.Type.Name);
            Assert.AreEqual("Null", BasicTypes.Null.Name);

            Assert.AreEqual("Number", BasicTypes.Number.Alias);
            Assert.AreEqual("String", BasicTypes.String.Alias);
            Assert.AreEqual("Date", BasicTypes.Date.Alias);
            Assert.AreEqual("Boolean", BasicTypes.Boolean.Alias);
            Assert.AreEqual("Undefined", BasicTypes.Undefined.Alias);
            Assert.AreEqual("Type", BasicTypes.Type.Alias);
            Assert.AreEqual("Null", BasicTypes.Null.Alias);
        }

        [TestMethod]
        public void TypeManager_BasicTypes()
        {
            var manager = new TypeManager();
            Assert.AreSame(BasicTypes.Number, manager["Number"]);
            Assert.AreSame(BasicTypes.String, manager["String"]);
            Assert.AreSame(BasicTypes.Date, manager["Date"]);
            Assert.AreSame(BasicTypes.Boolean, manager["Boolean"]);
            Assert.AreSame(BasicTypes.Undefined, manager["Undefined"]);
            Assert.AreSame(BasicTypes.Type, manager["Type"]);

            Assert.AreSame(BasicTypes.Number, manager["Число"]);
            Assert.AreSame(BasicTypes.String, manager["Строка"]);
            Assert.AreSame(BasicTypes.Date, manager["Дата"]);
            Assert.AreSame(BasicTypes.Boolean, manager["Булево"]);
            Assert.AreSame(BasicTypes.Undefined, manager["Неопределено"]);
            Assert.AreSame(BasicTypes.Type, manager["Тип"]);
        }

        [TestMethod]
        public void Types_Equality()
        {
            var id = TypeId.New();
            var manager1 = new TypeManager();
            var newType1 = manager1.RegisterType("ВидСравнения", "ComparisonType", id, null);

            var manager2 = new TypeManager();
            var newType2 = manager2.RegisterType("ВидСравнения", "ComparisonType", id, null);

            Assert.AreNotSame(newType1, newType2);
            Assert.IsTrue(newType1.Equals(newType2));
        }

        [TestMethod]
        public void TypeRegistration()
        {
            var manager = new TypeManager();
            var newType = manager.RegisterType("ВидСравнения", "ComparisonType");
            Assert.IsTrue(newType.Name == "ВидСравнения");
            Assert.IsTrue(newType.Alias == "ComparisonType");
            Assert.AreSame(newType, manager["ВидСравнения"]);

            newType = manager.RegisterType("Структура");
            Assert.IsTrue(newType.Name == "Структура");
            Assert.AreSame(newType, manager["Структура"]);

            Assert.IsTrue(TestHelpers.ExceptionThrown(
                ()=>manager.RegisterType("Структура"), 
                typeof(ArgumentException)), "No exception thrown");
            
        }
    }
}
