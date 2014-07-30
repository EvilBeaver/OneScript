using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Core;
using OneScript.ComponentModel;

namespace OneScript.Tests
{
    [TestClass]
    public class ComponentsTest
    {
        [TestMethod]
        public void Standard_ClassCreation_Test()
        {
            var manager = new TypeManager();
            var type = manager.RegisterObjectType("TestMock", null, CreatableInstanceMock.Constructor);

            IValue instance = type.CreateInstance(null);
            Assert.IsTrue(instance.GetType() == typeof(CreatableInstanceMock));
            Assert.AreSame(instance.Type, manager["TestMock"]);

            var numType = BasicTypes.Number;
            Assert.IsTrue(
                TestHelpers.ExceptionThrown(() => numType.CreateInstance(null),
                typeof(NotSupportedException)));
            
        }

        [TestMethod]
        public void Automated_Type_Import()
        {
            var manager = new TypeManager();
            var importer = new TypeImporter(manager);

            var newtype = importer.ImportType(typeof(CreatableInstanceMock));
            Assert.AreSame(newtype, manager["TestMock"]);

            var instance = newtype.CreateInstance(null);
            Assert.IsTrue(instance.GetType() == typeof(CreatableInstanceMock));
            Assert.IsTrue(instance.Type.Equals(newtype));
            
        }

        [TestMethod]
        public void Imported_Constructor_Choice()
        {
            var manager = new TypeManager();
            var importer = new TypeImporter(manager);
            var newtype = importer.ImportType(typeof(CreatableInstanceMock));
            var instance = (CreatableInstanceMock)newtype.CreateInstance(new IValue[]
                {
                    ValueFactory.Create("string"),
                    ValueFactory.Create(124)
                });
            Assert.IsTrue(instance.Type.Equals(newtype));
            Assert.IsTrue(instance.StringParam == "string");
            Assert.IsTrue(instance.IntegerParam == 124);

            bool thrown = false;
            try
            {
                instance = (CreatableInstanceMock)newtype.CreateInstance(new IValue[]
                {
                    ValueFactory.Create(true),
                    ValueFactory.Create()
                });
            }
            catch (EngineException)
            {
                thrown = true;
            }

            if (!thrown)
                Assert.Fail("Конструктор найден, хотя не должен быть");
        }

        [TestMethod]
        public void Basic_Imported_Functionality()
        {
            var manager = new TypeManager();
            var importer = new TypeImporter(manager);
            var newtype = importer.ImportType(typeof(CreatableInstanceMock));

            var instance = newtype.CreateInstance(new IValue[0]);
            Assert.IsTrue(instance.AsString() == newtype.Name);
            Assert.IsTrue(instance.ToString() == newtype.Name);

            try
            {
                instance.AsNumber();
            }
            catch (TypeConversionException e)
            {
                Assert.IsTrue(e.Message == "Преобразование к типу 'Число' не может быть выполнено");
            }

            try
            {
                instance.AsBoolean();
            }
            catch (TypeConversionException e)
            {
                Assert.IsTrue(e.Message == "Преобразование к типу 'Булево' не может быть выполнено");
            }

            try
            {
                instance.AsDate();
            }
            catch (TypeConversionException e)
            {
                Assert.IsTrue(e.Message == "Преобразование к типу 'Дата' не может быть выполнено");
            }

            try
            {
                instance.AsObject();
            }
            catch (TypeConversionException e)
            {
                Assert.IsTrue(e.Message == "Значение не является значением объектного типа");
            }
        }

        [TestMethod]
        public void Imported_Comparison()
        {
            var manager = new TypeManager();
            var importer = new TypeImporter(manager);
            var newtype = importer.ImportType(typeof(CreatableInstanceMock));

            var first = newtype.CreateInstance(new IValue[0]);
            var second = newtype.CreateInstance(new IValue[0]);
            Assert.AreNotEqual(first, second);
            Assert.IsFalse(first.Equals(second));

            int hashResult = first.GetHashCode() - second.GetHashCode();
            Assert.IsTrue(first.CompareTo(second) == hashResult);

        }

        [ImportedClass(Name="TestMock")]
        class CreatableInstanceMock : ComponentBase, IValue
        {
            public string StringParam { get; set; }
            public int IntegerParam { get; set; }

            public static IValue Constructor(DataType constructedType, IValue[] args)
            {
                var newObj = new CreatableInstanceMock();
                newObj.Type = constructedType;
                return newObj;
            }

            [TypeConstructor]
            public static CreatableInstanceMock CreateDefault()
            {
                var inst = new CreatableInstanceMock();
                return inst;
            }

            [TypeConstructor]
            public static CreatableInstanceMock CreateParametrized(string strArg, int intArg)
            {
                var inst = new CreatableInstanceMock();
                inst.StringParam = strArg;
                inst.IntegerParam = intArg;
                return inst;
            }

        }

        [ImportedClass]
        class ImportedMembersClass
        {
            [ContextProperty(Name = "ЧисловоеЗначение")]
            public int IntProperty { get; set; }

            [ContextProperty]
            public bool BooleanAutoName { get; set; }

            [ContextProperty(Name = "БулевоСвойство", Alias="BooleanProperty")]
            public bool BooleanExplicitName { get; set; }

            [ContextProperty(CanWrite=false)]
            public string ReadOnlyString { get; set; }
        }
    }
}
