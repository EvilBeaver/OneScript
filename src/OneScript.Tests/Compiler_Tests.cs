using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Scripting;
using System.Linq;

namespace OneScript.Tests
{
    [TestClass]
    public class Compiler_Tests
    {
        [TestMethod]
        public void ModuleImage_Initial_State()
        {
            var mi = new ModuleImage();
            Assert.IsTrue(mi.EntryMethodIndex == ModuleImage.INVALID_ADDRESS);
            Assert.IsNotNull(mi.Constants);
            Assert.IsNotNull(mi.Variables);
            Assert.IsNotNull(mi.Methods);
            Assert.IsNotNull(mi.MethodRefs);
            Assert.IsNotNull(mi.VariableRefs);
            Assert.IsNotNull(mi.Code);
            Assert.IsNull(mi.CodeIndexer);

            Assert.IsTrue(mi.Constants.Count == 0);
            Assert.IsTrue(mi.Variables.Count == 0);
            Assert.IsTrue(mi.Methods.Count == 0);
            Assert.IsTrue(mi.MethodRefs.Count == 0);
            Assert.IsTrue(mi.VariableRefs.Count == 0);
            Assert.IsTrue(mi.Code.Count == 0);
            
        }

        [TestMethod]
        public void Variable_Section_Is_Built()
        {
            var code = @"Перем А,Б,В;
                        Перем Г Экспорт;
                        Перем Д;";
            var iterator = new SourceCodeIterator(code);
            var context = new CompilerContext();

            ModuleImage module = new ModuleImage();
            var varBlockCompiler = new VariableBlockCompilerState();
            varBlockCompiler.Module = module;
            varBlockCompiler.Context = context;
            varBlockCompiler.Build();

            Assert.IsTrue(module.Variables.Count == 5);
            var names = module.Variables.Select((x) => x.Name);
            var namesConcat = String.Join(",", names);
            Assert.IsTrue(namesConcat == "А,Б,В,Г,Д");
            Assert.IsTrue(!module.Variables[0].IsExported);
            Assert.IsTrue(!module.Variables[1].IsExported);
            Assert.IsTrue(!module.Variables[2].IsExported);
            Assert.IsTrue(module.Variables[3].IsExported);
            Assert.IsTrue(!module.Variables[4].IsExported);
        }
    }
}
