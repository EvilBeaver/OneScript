using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Core;
using OneScript.ComponentModel;

namespace OneScript.Tests
{
    [TestClass]
    public class AutoContextTest
    {
        [TestMethod]
        public void Auto_Imported_Properties_Test()
        {
            var manager = new TypeManager();
            var importer = new TypeImporter(manager);
            var newtype = importer.ImportType(typeof(ImportedMembersClass));
            Assert.IsTrue(newtype.Name == "ImportedMembersClass");

            var ctx = (IRuntimeContextInstance)newtype.CreateInstance(new IValue[0]);
            Assert.IsTrue(ctx.GetPropCount() == 4);

            int index = ctx.FindProperty("ЧисловоеЗначение");
            Assert.IsTrue(ctx.GetPropertyName(index) == "ЧисловоеЗначение");
            ctx.SetPropertyValue(index, ValueFactory.Create(5));
            Assert.IsTrue(ctx.GetPropertyValue(index).Type == BasicTypes.Number);
            Assert.IsTrue(ctx.GetPropertyValue(index).AsNumber() == 5);
            Assert.IsTrue(ctx.IsPropReadable(index));
            Assert.IsTrue(ctx.IsPropWriteable(index));
            Assert.IsTrue(ctx.FindProperty("IntProperty") == index);

            index = ctx.FindProperty("BooleanAutoName");
            Assert.IsTrue(ctx.GetPropertyName(index) == "BooleanAutoName");
            Assert.IsTrue(ctx.GetPropertyName(index, NameRetrievalMode.Default) == "BooleanAutoName");
            Assert.IsTrue(ctx.GetPropertyName(index, NameRetrievalMode.Name) == "BooleanAutoName");
            Assert.IsTrue(ctx.GetPropertyName(index, NameRetrievalMode.PreferAlias) == "BooleanAutoName");
            Assert.IsTrue(ctx.GetPropertyName(index, NameRetrievalMode.OnlyAlias) == null);
            
            ctx.SetPropertyValue(index, ValueFactory.Create(true));
            Assert.IsTrue(ctx.GetPropertyValue(index).Type == BasicTypes.Boolean);
            Assert.IsTrue(ctx.GetPropertyValue(index).AsBoolean() == true);
            Assert.IsTrue(ctx.IsPropReadable(index));
            Assert.IsTrue(ctx.IsPropWriteable(index));

            index = ctx.FindProperty("BooleanProperty");
            Assert.IsTrue(ctx.GetPropertyName(index) == "БулевоСвойство");
            ctx.SetPropertyValue(index, ValueFactory.Create(true));
            Assert.IsTrue(ctx.GetPropertyValue(index).Type == BasicTypes.Boolean);
            Assert.IsTrue(ctx.GetPropertyValue(index).AsBoolean() == true);
            Assert.IsTrue(ctx.IsPropReadable(index));
            Assert.IsTrue(ctx.IsPropWriteable(index));

            index = ctx.FindProperty("ReadOnlyString");
            Assert.IsTrue(ctx.GetPropertyName(index) == "ReadOnlyString");
            ((ImportedMembersClass)ctx).ReadOnlyString = "it is a string";
            Assert.IsTrue(ctx.GetPropertyValue(index).Type == BasicTypes.String);
            Assert.IsTrue(ctx.GetPropertyValue(index).AsString() == "it is a string");
            Assert.IsTrue(ctx.IsPropReadable(index));
            Assert.IsFalse(ctx.IsPropWriteable(index));
            Assert.IsTrue(TestHelpers.ExceptionThrown(
                () => ctx.SetPropertyValue(index, ValueFactory.Create("dd")), 
                typeof(ContextAccessException)));
        }

        [TestMethod]
        public void Auto_Imported_Methods_Test()
        {
            var ctx = new ImportedMembersClass();

            int index = ctx.FindMethod("Процедура");
            Assert.IsTrue(ctx.GetParametersCount(index) == 0);
            Assert.IsTrue(ctx.HasReturnValue(index) == false);
            Assert.IsTrue(index == ctx.FindMethod("Proc"));
            ctx.CallAsFunction(index, new IValue[0]);

            index = ctx.FindMethod("SimpleFunction");
            Assert.IsTrue(ctx.GetParametersCount(index) == 0);
            Assert.IsTrue(ctx.HasReturnValue(index) == true);
            Assert.IsTrue(ctx.CallAsFunction(index, new IValue[0]).AsNumber() == 0);

            Assert.AreEqual("SimpleFunction", ctx.GetMethodName(index));
            Assert.AreEqual("SimpleFunction", ctx.GetMethodName(index, NameRetrievalMode.Default));
            Assert.AreEqual("SimpleFunction", ctx.GetMethodName(index, NameRetrievalMode.Name));
            Assert.AreEqual("SimpleFunction", ctx.GetMethodName(index, NameRetrievalMode.PreferAlias));
            Assert.AreEqual(null, ctx.GetMethodName(index, NameRetrievalMode.OnlyAlias));

            index = ctx.FindMethod("Функция");
            Assert.IsTrue(ctx.GetParametersCount(index) == 3);
            Assert.IsTrue(ctx.HasReturnValue(index) == true);
            IValue defValue;
            Assert.IsTrue(ctx.GetDefaultValue(index, 0, out defValue) == false);
            Assert.IsTrue(ctx.GetDefaultValue(index, 1, out defValue) == false);
            Assert.IsTrue(ctx.GetDefaultValue(index, 2, out defValue) == true);
            Assert.IsTrue(defValue.AsString() == "defParam");
            Assert.IsTrue(ctx.CallAsFunction(index, new IValue[3]).AsNumber() == 1);
        }

        [TestMethod]
        public void Context_Access_Exceptions()
        {
            var ctx = new ImportedMembersClass();
            Assert.IsTrue(TestHelpers.ExceptionThrown(() =>
            {
                ctx.FindMethod("NoSuchMethod");
            }, typeof(ContextAccessException)));

            Assert.IsTrue(TestHelpers.ExceptionThrown(() =>
            {
                ctx.FindProperty("NoSuchProp");
            }, typeof(ContextAccessException)));
        }

    }

    [ImportedClass]
    class ImportedMembersClass : AutoContext<ImportedMembersClass>
    {
        [ContextProperty(Name = "ЧисловоеЗначение")]
        public int IntProperty { get; set; }

        [ContextProperty]
        public bool BooleanAutoName { get; set; }

        [ContextProperty(Name = "БулевоСвойство", Alias = "BooleanProperty")]
        public bool BooleanExplicitName { get; set; }

        [ContextProperty(CanWrite = false)]
        public string ReadOnlyString { get; set; }

        [ContextMethod(Name = "Процедура", Alias = "Proc")]
        public void SimpleProcedure()
        { }

        [ContextMethod]
        public int SimpleFunction() { return 0; }

        [ContextMethod(Name = "Функция")]
        public int Func(int arg1, string arg2, string arg3 = "defParam")
        {
            return 1;
        }

        [TypeConstructor]
        public static IValue Constructor()
        {
            return new ImportedMembersClass();
        }
    }

}
