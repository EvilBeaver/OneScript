/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using OneScript.StandardLibrary;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Extensions;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;
using Xunit;

namespace OneScript.Core.Tests
{
    public class CodeStatProcessorTests
    {
        private const string ModuleName = "module.os";

        [Fact]
        public void CountsFromManyThreadsAddUp()
        {
            const int threadsCount = 16;
            const int iterations = 20000;
            var collector = new CodeStatProcessor();
            var entries = Enumerable.Range(1, 50).Select(i => new CodeStatEntry(ModuleName, "Метод", i)).ToArray();

            RunInParallel(threadsCount, () =>
            {
                // Все потоки впервые исполняют один и тот же модуль и регистрируют одни и те же точки
                foreach (var entry in entries)
                {
                    collector.MarkEntryReached(entry, count: 0);
                }
                collector.MarkPrepared(ModuleName);

                for (var i = 0; i < iterations; i++)
                {
                    collector.MarkEntryReached(entries[i % entries.Length]);
                }
            });

            collector.EndCodeStat();
            var data = collector.GetStatData();

            data.Should().HaveCount(entries.Length);
            data.Select(x => x.ExecutionCount).Should()
                .AllBeEquivalentTo(threadsCount * iterations / entries.Length);
        }

        [Fact]
        public void StatDataCanBeTakenWhileThreadsAreCounting()
        {
            var collector = new CodeStatProcessor();
            var stop = 0;
            var exceptions = new ConcurrentQueue<Exception>();

            var workers = Enumerable.Range(0, 8).Select(t => new Thread(() =>
            {
                try
                {
                    var line = 0;
                    while (Volatile.Read(ref stop) == 0)
                    {
                        // Новые точки появляются, пока уже зарегистрированные считаются
                        var entry = new CodeStatEntry(ModuleName, "Метод" + t, line++ % 1000);
                        collector.MarkEntryReached(entry, count: 0);
                        collector.MarkEntryReached(entry);
                    }
                }
                catch (Exception e)
                {
                    exceptions.Enqueue(e);
                }
            })).ToArray();

            collector.MarkPrepared(ModuleName);
            foreach (var worker in workers)
            {
                worker.Start();
            }

            try
            {
                for (var i = 0; i < 200; i++)
                {
                    collector.GetStatData();
                    collector.EndCodeStat();
                }
            }
            finally
            {
                Volatile.Write(ref stop, 1);
                foreach (var worker in workers)
                {
                    worker.Join();
                }
            }

            exceptions.Should().BeEmpty();
            collector.GetStatData().Should().HaveCount(8 * 1000);
        }

        [Fact]
        public void TimeIsAccountedToTheLastReachedEntry()
        {
            var collector = new CodeStatProcessor();
            var first = new CodeStatEntry(ModuleName, "Метод", 1);
            var second = new CodeStatEntry(ModuleName, "Метод", 2);
            collector.MarkEntryReached(first, count: 0);
            collector.MarkEntryReached(second, count: 0);
            collector.MarkPrepared(ModuleName);

            collector.MarkEntryReached(first);
            Thread.Sleep(100);
            collector.MarkEntryReached(second);
            collector.MarkEntryReached(first);
            Thread.Sleep(100);
            collector.EndCodeStat();

            var data = collector.GetStatData().ToDictionary(x => x.Entry.LineNumber);
            data[1].ExecutionCount.Should().Be(2);
            data[1].TimeElapsed.Should().BeGreaterOrEqualTo(190);
            data[2].ExecutionCount.Should().Be(1);
            data[2].TimeElapsed.Should().BeLessThan(50);
        }

        [Fact]
        public void StoppedEntryDoesNotAccumulateTime()
        {
            var collector = new CodeStatProcessor();
            var entry = new CodeStatEntry(ModuleName, "Метод", 1);
            collector.MarkEntryReached(entry, count: 0);
            collector.MarkPrepared(ModuleName);

            collector.MarkEntryReached(entry);
            collector.StopWatch(entry);
            Thread.Sleep(100);
            collector.EndCodeStat();

            collector.GetStatData().Single().TimeElapsed.Should().BeLessThan(50);
        }

        [Fact]
        public void StatDataKeepsRegistrationOrder()
        {
            var collector = new CodeStatProcessor();
            var entries = Enumerable.Range(1, 100)
                .Select(i => new CodeStatEntry("module" + (i % 7) + ".os", "Метод" + (i % 3), i))
                .ToArray();

            foreach (var entry in entries)
            {
                collector.MarkEntryReached(entry, count: 0);
                collector.MarkPrepared(entry.ScriptFileName);
            }

            collector.GetStatData().Select(x => x.Entry).Should().Equal(entries);
        }

        [Fact]
        public void UnregisteredEntryDoesNotBreakExecution()
        {
            var collector = new CodeStatProcessor();
            var entry = new CodeStatEntry(ModuleName, "Метод", 1);

            Action act = () => collector.MarkEntryReached(entry);

            act.Should().NotThrow();
        }

        [Fact]
        public void BackgroundTasksExecuteSameModulesUnderCodeStat()
        {
            const int tasksCount = 16;
            const int classesCount = 10;
            const int repeats = 200;

            var dir = Path.Combine(Path.GetTempPath(), "codestat-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                for (var i = 0; i < classesCount; i++)
                {
                    File.WriteAllText(Path.Combine(dir, $"Класс{i}.os"), ClassModule);
                }

                var collector = new CodeStatProcessor();
                var engine = CreateEngine(collector);

                var mainModule = MainModule
                    .Replace("%КАТАЛОГ%", dir)
                    .Replace("%ЗАДАНИЙ%", tasksCount.ToString())
                    .Replace("%КЛАССОВ%", classesCount.ToString())
                    .Replace("%ПОВТОРОВ%", repeats.ToString());

                RunScript(engine, mainModule);

                collector.EndCodeStat();
                var data = collector.GetStatData();
                for (var i = 0; i < classesCount; i++)
                {
                    var fileName = $"Класс{i}.os";
                    // Строка тела цикла в методе Посчитать
                    data.Single(x => x.Entry.ScriptFileName.EndsWith(fileName) && x.Entry.LineNumber == 5)
                        .ExecutionCount.Should().Be(tasksCount * repeats, fileName);
                }
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        [Fact]
        public void IdleTimeAfterProcessExitIsNotAccounted()
        {
            var collector = new CodeStatProcessor();
            var engine = CreateEngine(collector);

            // Как поток пула: процесс завершился, поток простаивает, потом исполняет следующий процесс
            RunScript(engine, "А = 1;");
            Thread.Sleep(200);
            RunScript(engine, "Б = 2;");
            collector.EndCodeStat();

            collector.GetStatData().Select(x => x.TimeElapsed).Should().OnlyContain(x => x < 100);
        }

        // Движок собирается так же, как в oscript с -codestat
        private static HostedScriptEngine CreateEngine(ICodeStatCollector collector)
        {
            var builder = DefaultEngineBuilder.Create()
                .SetDefaultOptions()
                .UseImports()
                .UseDefaultHosting()
                .SetupEnvironment(e => e.AddStandardLibrary());
            builder.Services.RegisterSingleton(collector);
            var engine = new HostedScriptEngine(builder.Build());
            engine.Initialize();
            return engine;
        }

        private static void RunScript(HostedScriptEngine engine, string code)
        {
            engine.Engine.AttachedScriptsFactory.LoadFromString(
                engine.GetCompilerService(), code, engine.Engine.NewProcess());
        }

        private const string ClassModule =
            "Перем Сумма;\n" +
            "\n" +
            "Процедура Посчитать(Повторов) Экспорт\n" +
            "\tДля Н = 1 По Повторов Цикл\n" +
            "\t\tСумма = Сумма + 1;\n" +
            "\tКонецЦикла;\n" +
            "КонецПроцедуры\n" +
            "\n" +
            "Сумма = 0;\n";

        private const string MainModule =
            "Перем Старт;\n" +
            "\n" +
            "Процедура Работа(НомерЗадания) Экспорт\n" +
            "\tПока Не Старт Цикл\n" +
            "\tКонецЦикла;\n" +
            "\tДля Н = 0 По %КЛАССОВ% - 1 Цикл\n" +
            "\t\tОбъект = Новый(\"Класс\" + Н);\n" +
            "\t\tОбъект.Посчитать(%ПОВТОРОВ%);\n" +
            "\t\tЗагрузитьСценарийИзСтроки(\"Перем Х; Х = \" + Формат(НомерЗадания * 100 + Н, \"ЧГ=\") + \";\");\n" +
            "\tКонецЦикла;\n" +
            "КонецПроцедуры\n" +
            "\n" +
            "Для Н = 0 По %КЛАССОВ% - 1 Цикл\n" +
            "\tПодключитьСценарий(ОбъединитьПути(\"%КАТАЛОГ%\", \"Класс\" + Н + \".os\"), \"Класс\" + Н);\n" +
            "КонецЦикла;\n" +
            "\n" +
            "Старт = Ложь;\n" +
            "Задания = Новый Массив;\n" +
            "Для Н = 1 По %ЗАДАНИЙ% Цикл\n" +
            "\tПараметры = Новый Массив;\n" +
            "\tПараметры.Добавить(Н);\n" +
            "\tЗадания.Добавить(ФоновыеЗадания.Выполнить(ЭтотОбъект, \"Работа\", Параметры));\n" +
            "КонецЦикла;\n" +
            "Старт = Истина;\n" +
            "ФоновыеЗадания.ОжидатьВсе(Задания);\n" +
            "\n" +
            "Ошибки = \"\";\n" +
            "Для Каждого Задание Из Задания Цикл\n" +
            "\tЕсли Задание.ИнформацияОбОшибке <> Неопределено Тогда\n" +
            "\t\tОшибки = Ошибки + Задание.ИнформацияОбОшибке.ПодробноеОписаниеОшибки() + Символы.ПС;\n" +
            "\tКонецЕсли;\n" +
            "КонецЦикла;\n" +
            "Если Ошибки <> \"\" Тогда\n" +
            "\tВызватьИсключение Ошибки;\n" +
            "КонецЕсли;\n";

        private static void RunInParallel(int threadsCount, Action action)
        {
            var exceptions = new ConcurrentQueue<Exception>();
            using var barrier = new Barrier(threadsCount);
            var threads = Enumerable.Range(0, threadsCount).Select(_ => new Thread(() =>
            {
                try
                {
                    barrier.SignalAndWait();
                    action();
                }
                catch (Exception e)
                {
                    exceptions.Enqueue(e);
                }
            })).ToArray();

            foreach (var thread in threads)
            {
                thread.Start();
            }
            foreach (var thread in threads)
            {
                thread.Join();
            }

            exceptions.Should().BeEmpty();
        }
    }
}
