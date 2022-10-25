/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using FluentAssertions;
using OneScript.Compilation;
using OneScript.Compilation.Binding;
using OneScript.DependencyInjection;
using OneScript.Execution;
using OneScript.Language;
using OneScript.Native.Compiler;
using OneScript.Native.Extensions;
using OneScript.Native.Runtime;
using OneScript.Sources;
using OneScript.StandardLibrary.Collections;
using OneScript.StandardLibrary.Collections.ValueTable;
using OneScript.StandardLibrary.TypeDescriptions;
using OneScript.Types;
using ScriptEngine;
using ScriptEngine.Compiler;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;
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
            testServices.RegisterSingleton<CompileTimeSymbolsProvider>();
            testServices.UseImports();
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

        [Fact]
        public void CanSelectNativeCompiler()
        {
            testServices.Register<IErrorSink, ThrowingErrorSink>();
            testServices.UseNativeRuntime();

            var code = 
                @"
                #native
                A = 1;
                N = 4;";
            
            var services = testServices.CreateContainer();
            var compiler = services.Resolve<CompilerFrontend>();

            var source = SourceCodeBuilder.Create().FromString(code).Build();
            var module = compiler.Compile(source);

            module.Should().BeOfType<DynamicModule>();
        }

        [Fact]
        public void Test_Can_Call_ForwardedDeclarationCall()
        {
            var symbols = new SymbolTable();
            var module = CreateModule(
                @"Процедура Процедура1()
	                Процедура2();
                КонецПроцедуры

                Процедура Процедура2()
                КонецПроцедуры

                Процедура1()", testServices.CreateContainer(), symbols);

            var sdo = new UserScriptContextInstance(module,
                new TypeDescriptor(new Guid(), "TestClass", default, typeof(UserScriptContextInstance)));
            sdo.InitOwnData();
            sdo.Initialize();
        }
        
        [Fact]
        public void Can_Assign_To_BslDictionary()
        {
            var code = string.Join('\n',
                "Соответствие = Новый Соответствие;",
                "Соответствие[\"1\"] = \"2\";"
            );

            var services = testServices.CreateContainer();
            services.Resolve<ITypeManager>().RegisterClass(typeof(MapImpl));
            CreateModule(code, services, new SymbolTable());
        }
        
        [Fact]
        public void Can_Read_From_BslDictionary()
        {
            var code = string.Join('\n',
                "Соответствие = Новый Соответствие;",
                "Соответствие.Вставить(\"1\",\"2\");",
                "Рез = Соответствие[\"1\"];"
            );

            var services = testServices.CreateContainer();
            services.Resolve<ITypeManager>().RegisterClass(typeof(MapImpl));
            CreateModule(code, services, new SymbolTable());
        }
        
        [Fact]
        public void NRE_At_Runtime_On_Method_Var_Issue1206()
        {
            var code =
                @"Процедура Проц()
                    Перем Лок;
                    Лок = 0;
                    Если Лок > 1 Тогда 
                    КонецЕсли;
                КонецПроцедуры

                Проц();";
            
            var module = CreateModule(code);
            var sdo = new UserScriptContextInstance(module,
                new TypeDescriptor(new Guid(), "TestClass", default, typeof(UserScriptContextInstance)));
            sdo.InitOwnData();
            sdo.Initialize();
        }
        
        [Fact]
        public void Integer_To_ClassField_assignment_In_Loop()
        {
            var code =
                @"Перем Индекс;
                Индекс = Неопределено;
                Для Индекс = 0 По 1 Цикл
                КонецЦикла;";
            
            var module = CreateModule(code);
            var sdo = new UserScriptContextInstance(module,
                new TypeDescriptor(new Guid(), "TestClass", default, typeof(UserScriptContextInstance)));
            sdo.InitOwnData();
            sdo.Initialize();
        }
        
        [Fact]
        public void Integer_To_Variant_assignment_In_Loop()
        {
            var code =
                @"Процедура Тест()
                    Перем Индекс;
                    Индекс = Неопределено;
                    Для Индекс = 0 По 1 Цикл
                    КонецЦикла;
                КонецПроцедуры";
            
            var module = CreateModule(code);
            var sdo = new UserScriptContextInstance(module,
                new TypeDescriptor(new Guid(), "TestClass", default, typeof(UserScriptContextInstance)));
            sdo.InitOwnData();
            sdo.Initialize();
        }

        [Fact]
        public void Call_Proc_On_Variants_Chain()
        {
            var code =
            @"Процедура Проц()
                  Перем Таблица;
                  Таблица = Новый ТаблицаЗначений;
                  Таблица.Колонки.Добавить(""Имя"", Новый ОписаниеТипов(""Строка""));
              КонецПроцедуры";
            
            var services = testServices.CreateContainer();
            var typeManager = services.Resolve<ITypeManager>(); 
            typeManager.RegisterClass(typeof(ValueTable));
            typeManager.RegisterClass(typeof(ValueTableColumnCollection));
            typeManager.RegisterClass(typeof(ValueTableColumn));
            typeManager.RegisterClass(typeof(TypeDescription));
            CreateModule(code, services, new SymbolTable());
        }

        private DynamicModule CreateModule(string code) => CreateModule(code, testServices.CreateContainer(), new SymbolTable());
        
        private DynamicModule CreateModule(string code, IServiceContainer services, SymbolTable symbols)
        {
            var symbolProvider = services.Resolve<CompileTimeSymbolsProvider>();
            var moduleScope = new SymbolScope();
            symbolProvider.Get<UserScriptContextInstance>().FillSymbols(moduleScope);
            symbols.PushScope(moduleScope, null);
            
            var helper = new CompileHelper(services);
            helper.ParseModule(code);
            var result = helper.Compile(symbols);
            helper.ThrowOnErrors();
            return result;
        }
    }
}
