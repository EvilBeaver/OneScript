/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Perfolizer.Horology;

namespace OneScript.Benchmarks
{
    internal static class Program
    {
        private const string ENGINE_OPTION = "--engine";

        /// <summary>
        /// Аргументы BenchmarkDotNet передаются как есть. Кроме них можно указать
        /// --engine имя=путь/к/src - еще одну версию движка для сравнения (опцию можно повторять).
        /// Базовой версией считается движок из этого же репозитория.
        /// </summary>
        public static void Main(string[] args)
        {
            var engines = new List<(string Name, string SourcePath)>();
            var benchmarkArgs = new List<string>();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == ENGINE_OPTION && i + 1 < args.Length)
                {
                    var parts = args[++i].Split('=', 2);
                    if (parts.Length != 2 || parts[0].Length == 0 || parts[1].Length == 0)
                        throw new ArgumentException($"Ожидается {ENGINE_OPTION} имя=путь/к/src, получено '{args[i]}'");

                    // Без завершающего разделителя: иначе "\" перед закрывающей кавычкой в аргументе MSBuild ее экранирует
                    engines.Add((parts[0], Path.TrimEndingDirectorySeparator(Path.GetFullPath(parts[1]))));
                }
                else
                {
                    benchmarkArgs.Add(args[i]);
                }
            }

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
                .Run(benchmarkArgs.ToArray(), CreateConfig(engines));
        }

        private static IConfig CreateConfig(List<(string Name, string SourcePath)> engines)
        {
            var config = DefaultConfig.Instance;
            if (engines.Count == 0)
                return config;

            // Несколько запусков: JIT от процесса к процессу может скомпилировать горячий путь по-разному
            var job = Job.Default
                .WithLaunchCount(3)
                .WithWarmupCount(6)
                .WithIterationCount(15)
                .WithIterationTime(TimeInterval.FromMilliseconds(250));

            config = config
                .WithOrderer(new InterleavedEnginesOrderer())
                .AddJob(job.WithId("current").AsBaseline());
            foreach (var (name, sourcePath) in engines)
            {
                // Кавычки - для путей с пробелами: BenchmarkDotNet передает аргумент в dotnet build как есть
                config = config.AddJob(job
                    .WithId(name)
                    .WithArguments(new[] { new MsBuildArgument($"/p:OneScriptSrc=\"{sourcePath}\"") }));
            }

            return config;
        }
    }
}
