/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.StandardLibrary.Collections;
using OneScript.Types;
using ScriptEngine;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using Xunit;

namespace OneScript.Core.Tests
{
    public class UserScriptInitializationTests
    {
        [Fact]
        public void ConstructorReceivesArgumentsAndDefaultsOnlyOnce()
        {
            const string code = @"
                Перем ВызововТела Экспорт;
                Перем ВызововКонструктора Экспорт;
                Перем ПолученныйАргумент Экспорт;
                Перем ЗначениеПоУмолчанию Экспорт;

                Процедура ПриСозданииОбъекта(Знач Аргумент, Знач Необязательный = 42)
                    ВызововКонструктора = ВызововКонструктора + 1;
                    ПолученныйАргумент = Аргумент;
                    ЗначениеПоУмолчанию = Необязательный;
                КонецПроцедуры

                Если ВызововТела = Неопределено Тогда
                    ВызововТела = 0;
                    ВызововКонструктора = 0;
                КонецЕсли;
                ВызововТела = ВызововТела + 1;";

            var argument = new ArrayImpl();
            var arguments = new IValue[] { argument };
            var (engine, instance) = CreateInstance(code, arguments);

            engine.InitializeSDO(instance, engine.NewProcess());

            instance.GetPropValue(instance.GetPropertyNumber("ПолученныйАргумент")).Should().BeSameAs(argument);
            instance.GetPropValue(instance.GetPropertyNumber("ЗначениеПоУмолчанию")).AsNumber().Should().Be(42);
            arguments[0].Should().BeSameAs(argument, "массив вызывающего кода не должен изменяться");

            Action initializeAgain = () => engine.InitializeSDO(instance, engine.NewProcess());

            initializeAgain.Should().Throw<InvalidOperationException>();
            instance.GetPropValue(instance.GetPropertyNumber("ВызововТела")).AsNumber().Should().Be(1);
            instance.GetPropValue(instance.GetPropertyNumber("ВызововКонструктора")).AsNumber().Should().Be(1);
        }

        [Theory]
        [InlineData("Процедура ПриСозданииОбъекта(Знач Аргумент) КонецПроцедуры", false)]
        [InlineData("Процедура ПриСозданииОбъекта(Знач Аргумент, Знач Второй = 42) КонецПроцедуры", false)]
        [InlineData("Процедура ПриСозданииОбъекта(Знач Аргумент)\n" +
            "ВызватьИсключение \"Ошибка конструктора\"; КонецПроцедуры", true)]
        [InlineData("Процедура ПриСозданииОбъекта(Знач Аргумент) КонецПроцедуры\n" +
            "ВызватьИсключение \"Ошибка тела\";", true)]
        [InlineData("Перем Поле;", true)]
        [InlineData("Процедура ПриСозданииОбъекта() КонецПроцедуры", true)]
        public void ConstructorArgumentsCanBeCollectedAfterInitialization(string code, bool fails)
        {
            var (instance, arguments, argument) = InitializeWithWeakReferences(code, fails);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            arguments.IsAlive.Should().BeFalse("экземпляр больше не должен удерживать массив аргументов");
            argument.IsAlive.Should().BeFalse("не сохранённый скриптом аргумент должен освобождаться");

            GC.KeepAlive(instance);
        }

        [Fact]
        public void InitializationCannotBeRetriedAfterFailure()
        {
            var instance = new InitializationProbe
            {
                OnCreate = () => throw new ApplicationException("Initialization failed")
            };
            Action initialize = () => instance.Initialize(ForbiddenBslProcess.Instance);

            initialize.Should().Throw<ApplicationException>();
            initialize.Should().Throw<InvalidOperationException>();
            instance.Calls.Should().Be(1);
        }

        [Fact]
        public void InitializationCannotBeReentered()
        {
            var instance = new InitializationProbe();
            instance.OnCreate = () =>
            {
                if (instance.Calls == 1)
                {
                    Action initializeAgain = () => instance.Initialize(ForbiddenBslProcess.Instance);

                    initializeAgain.Should().Throw<InvalidOperationException>();
                }
            };

            instance.Initialize(ForbiddenBslProcess.Instance);

            instance.Calls.Should().Be(1);
        }

        [Fact]
        public async Task InitializationCannotRunConcurrently()
        {
            using var entered = new ManualResetEventSlim();
            using var release = new ManualResetEventSlim();
            var instance = new InitializationProbe();
            instance.OnCreate = () =>
            {
                if (instance.Calls == 1)
                {
                    entered.Set();
                    release.Wait(TimeSpan.FromSeconds(10)).Should().BeTrue();
                }
            };

            var first = Task.Run(() => instance.Initialize(ForbiddenBslProcess.Instance));

            try
            {
                entered.Wait(TimeSpan.FromSeconds(10)).Should().BeTrue();
                Action initializeAgain = () => instance.Initialize(ForbiddenBslProcess.Instance);

                initializeAgain.Should().Throw<InvalidOperationException>();
            }
            finally
            {
                release.Set();
                await first.WaitAsync(TimeSpan.FromSeconds(10));
            }

            instance.Calls.Should().Be(1);
        }

        private static (ScriptingEngine Engine, UserScriptContextInstance Instance) CreateInstance(
            string code, IValue[] arguments)
        {
            var engine = DefaultEngineBuilder.Create().SetDefaultOptions().Build();
            engine.Initialize();

            var module = engine.AttachedScriptsFactory.CompileModuleFromSource(
                engine.GetCompilerService(), engine.Loader.FromString(code), null, engine.NewProcess());
            var type = new TypeDescriptor(typeof(UserScriptContextInstance), "TestScript");
            var instance = new UserScriptContextInstance(module, type, arguments);
            instance.InitOwnData();

            return (engine, instance);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static (UserScriptContextInstance Instance, WeakReference Arguments, WeakReference Argument)
            InitializeWithWeakReferences(string code, bool fails)
        {
            var argument = new ArrayImpl();
            var arguments = new IValue[] { argument };
            var argumentsReference = new WeakReference(arguments);
            var argumentReference = new WeakReference(argument);
            var (engine, instance) = CreateInstance(code, arguments);
            Action initialize = () => instance.Initialize(engine.NewProcess());

            if (fails)
                initialize.Should().Throw<RuntimeException>();
            else
                initialize();

            return (instance, argumentsReference, argumentReference);
        }

        private sealed class InitializationProbe : ScriptDrivenObject
        {
            private int _calls;

            public Action OnCreate { get; set; }

            public int Calls => _calls;

            protected override int GetOwnVariableCount() => 0;

            protected override int GetOwnMethodCount() => 0;

            protected override void OnInstanceCreation(IBslProcess process)
            {
                Interlocked.Increment(ref _calls);

                OnCreate?.Invoke();
            }
        }
    }
}
