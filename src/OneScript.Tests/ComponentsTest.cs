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

        [ImportedClass(Name="TestMock")]
        class CreatableInstanceMock : ImportedClassBase, IValue
        {
            public string StringParam { get; set; }
            public int IntegerParam { get; set; }

            public static IValue Constructor(DataType constructedType, IValue[] args)
            {
                var newObj = new CreatableInstanceMock();
                newObj.SetDataTypeInternal(constructedType);
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

            public DataType Type
            {
                get { return GetDataTypeInternal(); }
            }

            public double AsNumber()
            {
                throw new NotImplementedException();
            }

            public string AsString()
            {
                throw new NotImplementedException();
            }

            public DateTime AsDate()
            {
                throw new NotImplementedException();
            }

            public bool AsBoolean()
            {
                throw new NotImplementedException();
            }

            public IRuntimeContextInstance AsObject()
            {
                throw new NotImplementedException();
            }

            public bool Equals(IValue other)
            {
                throw new NotImplementedException();
            }

            public int CompareTo(IValue other)
            {
                throw new NotImplementedException();
            }
        }
    }
}
