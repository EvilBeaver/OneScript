/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using FluentAssertions;
using OneScript.Contexts;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using Xunit;

namespace OneScript.Core.Tests
{
    [GlobalContext(ManualRegistration = true)]
    public class GlobalSymbolsTestContext : GlobalContextBase<GlobalSymbolsTestContext>
    {
    }

    public class GlobalSymbolsThreadSafetyTests
    {
        [Fact]
        public void ScriptsCompileWhileGlobalSymbolsAreAdded()
        {
            var engine = DefaultEngineBuilder.Create().SetDefaultOptions().Build();
            engine.Initialize();
            engine.Environment.InjectGlobalProperty(ValueFactory.Create("есть"), "ИзвестноеСвойство", true);

            // Много обращений к глобальному свойству: каждое ищет имя в общей области глобальных свойств
            var code = string.Concat(Enumerable.Repeat("А = ИзвестноеСвойство;\n", 200));
            var errors = new ConcurrentQueue<string>();
            var stop = false;
            var compiled = 0;
            const int compilersCount = 4;
            var compilers = Enumerable.Range(0, compilersCount).Select(_ => new Thread(() =>
            {
                while (!Volatile.Read(ref stop))
                {
                    try
                    {
                        var source = engine.Loader.FromString(code);
                        engine.GetCompilerService().Compile(source, engine.NewProcess());
                    }
                    catch (Exception e)
                    {
                        errors.Enqueue($"{e.GetType().Name}: {e.Message}");
                    }
                    Interlocked.Increment(ref compiled);
                }
            }) { IsBackground = true }).ToArray();

            foreach (var thread in compilers)
            {
                thread.Start();
            }

            // Компиляторы уже работают, когда начинается регистрация
            SpinWait.SpinUntil(() => Volatile.Read(ref compiled) >= compilersCount, 10000).Should().BeTrue();
            var compiledBeforeRegistration = Volatile.Read(ref compiled);

            // Как загрузка библиотеки и ПодключитьВнешнююКомпоненту в другом потоке
            for (var i = 0; i < 30000; i++)
            {
                engine.Environment.InjectGlobalProperty(ValueFactory.Create(i), "Свойство" + i, true);
                if (i % 1000 == 0)
                    engine.Environment.InjectObject(new GlobalSymbolsTestContext());
            }

            var compiledDuringRegistration = Volatile.Read(ref compiled) - compiledBeforeRegistration;

            Volatile.Write(ref stop, true);
            foreach (var thread in compilers)
            {
                thread.Join();
            }

            compiledDuringRegistration.Should().BePositive("компиляции должны идти вперемежку с регистрацией");
            errors.Should().BeEmpty();
        }
    }
}
