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
        public void BasicTypes_AreNonObject()
        {
            Assert.IsFalse(BasicTypes.Number.IsObject);
            Assert.IsFalse(BasicTypes.String.IsObject);
            Assert.IsFalse(BasicTypes.Date.IsObject);
            Assert.IsFalse(BasicTypes.Boolean.IsObject);
            Assert.IsFalse(BasicTypes.Undefined.IsObject);
            Assert.IsFalse(BasicTypes.Type.IsObject);
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

            Assert.AreEqual("Number", BasicTypes.Number.Alias);
            Assert.AreEqual("String", BasicTypes.String.Alias);
            Assert.AreEqual("Date", BasicTypes.Date.Alias);
            Assert.AreEqual("Boolean", BasicTypes.Boolean.Alias);
            Assert.AreEqual("Undefined", BasicTypes.Undefined.Alias);
            Assert.AreEqual("Type", BasicTypes.Type.Alias);
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
        public void TypeRegistration()
        {
            var manager = new TypeManager();
            var newType = manager.RegisterSimpleType("ВидСравнения", "ComparisonType");
            Assert.IsTrue(newType.Name == "ВидСравнения");
            Assert.IsTrue(newType.Alias == "ComparisonType");
            Assert.IsFalse(newType.IsObject);
            Assert.AreSame(newType, manager["ВидСравнения"]);

            newType = manager.RegisterObjectType("Структура");
            Assert.IsTrue(newType.Name == "Структура");
            Assert.IsTrue(newType.IsObject);
            Assert.AreSame(newType, manager["Структура"]);

            Assert.IsTrue(TestHelpers.ExceptionThrown(
                ()=>manager.RegisterObjectType("Структура"), 
                typeof(ArgumentException)), "No exception thrown");
            
        }
    }
}
