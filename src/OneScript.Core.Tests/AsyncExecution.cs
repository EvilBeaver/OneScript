/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Threading.Tasks;
using OneScript.Sources;
using OneScript.StandardLibrary;
using ScriptEngine;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;
using Xunit;

namespace OneScript.Core.Tests
{
    public class AsyncExecution
    {
        [Fact]
        public async Task RunInAsyncSimpleCode()
        {
            var engine = MakeTestEngine();
            engine.Initialize();

            var module = CompileModule(engine, "А = 1; Б = 2;");
            
            var sdo = engine.CreateUninitializedSDO(module);
            await engine.InitializeSDOAsync(sdo);
        }
        
        [Fact]
        public async Task RunMultipleTasks()
        {
            var engine = MakeTestEngine();
            engine.Initialize();

            var module = CompileModule(engine, 
                "Для Сч = 1 По 3 Цикл\n" +
                    "Приостановить(1000);\n" +
                    "КонецЦикла;");

            var tasks = new Task[3];
            for (int i = 0; i < tasks.Length; i++)
            {
                var sdo = engine.CreateUninitializedSDO(module);
                tasks[i] = engine.InitializeSDOAsync(sdo);
            }

            await Task.WhenAll(tasks);
        }
        
        private IExecutableModule CompileModule(ScriptingEngine engine, string code)
        {
            var codeSource = engine.Loader.FromString(code);
            var compiler = engine.GetCompilerService();
            return compiler.Compile(codeSource);
        }
        
        private ScriptingEngine MakeTestEngine()
        {
            var builder = DefaultEngineBuilder
                .Create()
                .SetDefaultOptions()
                .SetupEnvironment(e => e.AddStandardLibrary());
            return builder.Build();
        }
    }
}