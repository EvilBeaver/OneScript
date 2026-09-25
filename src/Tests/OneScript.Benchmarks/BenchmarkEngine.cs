/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts;
using OneScript.Execution;
using OneScript.StandardLibrary;
using ScriptEngine;
using ScriptEngine.Hosting;

namespace OneScript.Benchmarks
{
    /// <summary>
    /// Движок со стандартной библиотекой и загруженный из строки модуль, методы которого вызывают бенчмарки
    /// </summary>
    internal sealed class BenchmarkEngine : IDisposable
    {
        private readonly ScriptingEngine _engine;

        private BenchmarkEngine(ScriptingEngine engine, IBslProcess process, IRuntimeContextInstance module)
        {
            _engine = engine;
            Process = process;
            Module = module;
        }

        public IBslProcess Process { get; }

        public IRuntimeContextInstance Module { get; }

        public static BenchmarkEngine Load(string source)
        {
            var builder = DefaultEngineBuilder.Create()
                .SetDefaultOptions()
                .UseBinaryDataOptions();
            builder.SetupEnvironment(environment => environment.AddStandardLibrary());

            var engine = builder.Build();
            engine.Initialize();

            var process = engine.NewProcess();
            var module = engine.AttachedScriptsFactory.LoadFromString(engine.GetCompilerService(), source, process);

            return new BenchmarkEngine(engine, process, module);
        }

        public void Dispose()
        {
            _engine.Dispose();
        }
    }
}
