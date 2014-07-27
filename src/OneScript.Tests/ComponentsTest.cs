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
            
        }

        [TestMethod]
        public void Imported_Constructor_Choice()
        {

        }

        [ImportedClass(Name="TestMock")]
        class CreatableInstanceMock : IValue
        {
            DataType _type;

            public string StringParam { get; set; }
            public int IntegerParam { get; set; }

            public static IValue Constructor(DataType constructedType, IValue[] args)
            {
                var newObj = new CreatableInstanceMock();
                newObj._type = constructedType;
                return newObj;
            }

            [TypeConstructor]
            public static CreatableInstanceMock CreateDefault(DataType instanceType)
            {
                var inst = new CreatableInstanceMock();
                inst._type = instanceType;
                return inst;
            }

            [TypeConstructor]
            public static CreatableInstanceMock CreateParametrized(DataType instanceType, string strArg, int intArg)
            {
                var inst = new CreatableInstanceMock();
                inst._type = instanceType;
                inst.StringParam = strArg;
                inst.IntegerParam = intArg;
                return inst;
            }

            public DataType Type
            {
                get { return _type; }
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
