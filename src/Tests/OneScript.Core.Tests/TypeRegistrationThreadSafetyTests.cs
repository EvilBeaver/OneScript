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
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using Xunit;

namespace OneScript.Core.Tests
{
    public class TypeRegistrationThreadSafetyTests
    {
        [Fact]
        public void TypesRegisteredFromManyThreadsAreFoundByName()
        {
            const int threadsCount = 16;
            const int typesPerThread = 500;
            var typeManager = new DefaultTypeManager();
            var builtInCount = typeManager.RegisteredTypes().Count;

            RunInParallel(threadsCount, thread =>
            {
                for (var i = 0; i < typesPerThread; i++)
                {
                    var registered = typeManager.RegisterType($"Тип{thread}_{i}", $"Type{thread}_{i}", typeof(BslValue));

                    typeManager.GetTypeByName($"Тип{thread}_{i}").Should().BeSameAs(registered);
                    typeManager.GetTypeByName($"Type{thread}_{i}").Should().BeSameAs(registered);
                    // Другие потоки в это время регистрируют свои типы
                    typeManager.RegisteredTypes().Should().Contain(registered);
                }
            });

            typeManager.RegisteredTypes().Should().HaveCount(builtInCount + threadsCount * typesPerThread);
            for (var thread = 0; thread < threadsCount; thread++)
            {
                for (var i = 0; i < typesPerThread; i++)
                {
                    typeManager.GetTypeByName($"Type{thread}_{i}").Name.Should().Be($"Тип{thread}_{i}");
                }
            }
        }

        [Fact]
        public void SameTypeRegisteredConcurrentlyGetsOneDescriptor()
        {
            const int threadsCount = 8;
            var typeManager = new DefaultTypeManager();

            for (var round = 0; round < 200; round++)
            {
                var name = $"Общий{round}";
                var results = new ConcurrentBag<TypeDescriptor>();

                RunInParallel(threadsCount, _ => results.Add(typeManager.RegisterType(name, default, typeof(BslValue))));

                results.Distinct().Should().ContainSingle(name);
                typeManager.RegisteredTypes().Count(x => x.Name == name).Should().Be(1, name);
            }
        }

        [Fact]
        public void ScriptsAttachedFromManyThreadsAreRegisteredOnce()
        {
            const int threadsCount = 16;
            const int classesPerThread = 30;
            var engine = DefaultEngineBuilder.Create()
                .SetDefaultOptions()
                .Build();
            engine.Initialize();

            RunInParallel(threadsCount, thread =>
            {
                var process = engine.NewProcess();
                for (var i = 0; i < classesPerThread; i++)
                {
                    // Общий класс подключают все потоки, пока каждый подключает и свои классы
                    engine.AttachedScriptsFactory.AttachFromString(engine.GetCompilerService(),
                        "Перем Общая;", "ОбщийКласс", process);
                    engine.AttachedScriptsFactory.AttachFromString(engine.GetCompilerService(),
                        $"Перем Номер{i};", $"Класс{thread}_{i}", process);
                }
            });

            engine.TypeManager.RegisteredTypes().Count(x => x.Name == "ОбщийКласс").Should().Be(1);
            for (var thread = 0; thread < threadsCount; thread++)
            {
                for (var i = 0; i < classesPerThread; i++)
                {
                    engine.TypeManager.GetTypeByName($"Класс{thread}_{i}").ImplementingClass
                        .Should().Be(typeof(AttachedScriptsFactory));
                }
            }
        }

        private static void RunInParallel(int threadsCount, Action<int> action)
        {
            var exceptions = new ConcurrentQueue<Exception>();
            using var barrier = new Barrier(threadsCount);
            var threads = Enumerable.Range(0, threadsCount).Select(number => new Thread(() =>
            {
                try
                {
                    barrier.SignalAndWait();
                    action(number);
                }
                catch (Exception e)
                {
                    exceptions.Enqueue(e);
                }
            }) { IsBackground = true }).ToArray();

            foreach (var thread in threads)
            {
                thread.Start();
            }
            foreach (var thread in threads)
            {
                // Испорченный словарь может зациклить поток
                thread.Join(TimeSpan.FromSeconds(60)).Should().BeTrue("поток должен завершиться");
            }

            exceptions.Should().BeEmpty();
        }
    }
}
