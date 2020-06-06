/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Threading.Tasks;
using NUnit.Framework;
using ScriptEngine;

namespace NUnitTests
{
    [TestFixture]
    public class AsyncExecution
    {
        [Test]
        public async Task RunInAsyncSimpleCode()
        {
            var e = new ScriptingEngine();
            e.Initialize();
            var code = e.Loader.FromString("А = 1; Б = 2;");

            var compiler = e.GetCompilerService();
            var module = e.LoadModuleImage(compiler.Compile(code));
            e.Environment.LoadMemory(e.Machine);
            var sdo = e.CreateUninitializedSDO(module);
            await e.InitializeSDOAsync(sdo);
        }
    }
}