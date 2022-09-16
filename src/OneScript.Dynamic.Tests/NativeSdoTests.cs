using System;
using FluentAssertions;
using OneScript.Compilation.Binding;
using OneScript.Execution;
using OneScript.Native.Compiler;
using OneScript.Native.Extensions;
using OneScript.Native.Runtime;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using Xunit;

namespace OneScript.Dynamic.Tests
{
    public class NativeSdoTests
    {
        public NativeSdoTests()
        {
            try
            {
                ExecutionDispatcher.Current = new ExecutionDispatcher(new[] { new NativeExecutorProvider() });
            }
            catch (InvalidOperationException)
            {
            }
            
        }

        [Fact]
        public void TestCanInstantiateSdoWithNativeModule()
        {
            var module = CreateModule(
                @"Перем Ы;

                Процедура А()
                    Б = 1;
                КонецПроцедуры

                Процедура Б()
                    Б = 1;
                КонецПроцедуры

                В = 4;
                ");

            var sdo = new UserScriptContextInstance(module,
                new TypeDescriptor(new Guid(), "TestClass", default, typeof(UserScriptContextInstance)));
            
            sdo.InitOwnData();
            sdo.Initialize();
        }
        
        [Fact]
        public void Test_Local_Method_Can_Be_Called_and_Field_Set()
        {
            var symbols = new SymbolTable();
            symbols.PushScope(new SymbolScope(), null);
            UserScriptContextInstance.PrepareCompilation(symbols);
            var module = CreateModule(
                @"Перем Ы Экспорт;

                Процедура А() Экспорт
                    Ы = 1;
                КонецПроцедуры

                А();
                ", symbols);

            var sdo = new UserScriptContextInstance(module,
                new TypeDescriptor(new Guid(), "TestClass", default, typeof(UserScriptContextInstance)));
            sdo.InitOwnData();
            sdo.Initialize();
            var n = sdo.GetPropertyNumber("Ы");
            var val = sdo.GetPropValue(n);
            val.SystemType.Should().Be(BasicTypes.Number);
            val.AsNumber().Should().Be(1);
        }

        private DynamicModule CreateModule(string code) => CreateModule(code, new SymbolTable());
        
        private DynamicModule CreateModule(string code, SymbolTable symbols)
        {
            var helper = new CompileHelper();
            helper.ParseModule(code);
            return helper.Compile(symbols);
        }
    }
}