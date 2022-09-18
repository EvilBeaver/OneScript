using System;
using FluentAssertions;
using OneScript.Compilation.Binding;
using OneScript.DependencyInjection;
using OneScript.Execution;
using OneScript.Native.Compiler;
using OneScript.Native.Runtime;
using OneScript.StandardLibrary.Collections;
using OneScript.Types;
using ScriptEngine;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using Xunit;

namespace OneScript.Dynamic.Tests
{
    public class NativeSdoTests
    {
        private IServiceDefinitions testServices;
        
        public NativeSdoTests()
        {
            try
            {
                ExecutionDispatcher.Current = new ExecutionDispatcher(new[] { new NativeExecutorProvider() });
            }
            catch (InvalidOperationException)
            {
            }

            testServices = new TinyIocImplementation();
            testServices.Register(sp => sp);
            testServices.RegisterSingleton<ITypeManager, DefaultTypeManager>();
            testServices.RegisterSingleton<IGlobalsManager, GlobalInstancesManager>();
        }
        
        [Fact]
        public void Test_Can_Instantiate_Sdo_With_NativeModule()
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
                ", testServices.CreateContainer(), symbols);

            var sdo = new UserScriptContextInstance(module,
                new TypeDescriptor(new Guid(), "TestClass", default, typeof(UserScriptContextInstance)));
            sdo.InitOwnData();
            sdo.Initialize();
            var n = sdo.GetPropertyNumber("Ы");
            var val = sdo.GetPropValue(n);
            val.SystemType.Should().Be(BasicTypes.Number);
            val.AsNumber().Should().Be(1);
        }
        
        [Fact]
        public void Test_Can_Pass_Arguments_To_Local_Method()
        {
            var symbols = new SymbolTable();
            symbols.PushScope(new SymbolScope(), null);
            UserScriptContextInstance.PrepareCompilation(symbols);

            var serviceContainer = testServices.CreateContainer();
            var discoverer = serviceContainer.Resolve<ContextDiscoverer>();
            discoverer.DiscoverClasses(typeof(StructureImpl).Assembly);
            var module = CreateModule(
                @"Перем Результат Экспорт;

                Процедура Тест(А, Б, В, Г)
                    Результат = Новый Структура(""А,Б,В,Г"", А,Б,В,Г);
                КонецПроцедуры

                Тест(1, 2, 3, 4);
                ", serviceContainer, symbols);

            var sdo = new UserScriptContextInstance(module,
                new TypeDescriptor(new Guid(), "TestClass", default, typeof(UserScriptContextInstance)));
            sdo.InitOwnData();
            sdo.Initialize();
            var n = sdo.GetPropertyNumber("Результат");
            var val = sdo.GetPropValue(n) as StructureImpl;
            val.Should().NotBeNull();

            val.GetPropValue("А").AsNumber().Should().Be(1);
            val.GetPropValue("Б").AsNumber().Should().Be(2);
            val.GetPropValue("В").AsNumber().Should().Be(3);
            val.GetPropValue("Г").AsNumber().Should().Be(4);

        }
        
        [Fact]
        public void Test_Can_Read_Module_Variables()
        {
            var symbols = new SymbolTable();
            symbols.PushScope(new SymbolScope(), null);
            UserScriptContextInstance.PrepareCompilation(symbols);
            var module = CreateModule(
                @"Перем М Экспорт;

                Процедура А() Экспорт
                    М = М + 20;
                КонецПроцедуры

                М = 1;
                А();
                ", testServices.CreateContainer(), symbols);

            var sdo = new UserScriptContextInstance(module,
                new TypeDescriptor(new Guid(), "TestClass", default, typeof(UserScriptContextInstance)));
            sdo.InitOwnData();
            sdo.Initialize();
            var n = sdo.GetPropertyNumber("М");
            var val = sdo.GetPropValue(n);
            val.SystemType.Should().Be(BasicTypes.Number);
            val.AsNumber().Should().Be(21);
        }

        private DynamicModule CreateModule(string code) => CreateModule(code, testServices.CreateContainer(), new SymbolTable());
        
        private DynamicModule CreateModule(string code, IServiceContainer services, SymbolTable symbols)
        {
            var helper = new CompileHelper(services);
            helper.ParseModule(code);
            var result = helper.Compile(symbols);
            helper.ThrowOnErrors();
            return result;
        }
    }
}