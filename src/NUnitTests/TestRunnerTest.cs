/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.IO;
using NUnit.Framework;

namespace NUnitTests
{
    [TestFixture]
    public class UnitTestWrapper
    {
        private const int TEST_STATE_FAILED = 3;

        private EngineWrapperNUnit host;
        private string solutionRoot;

        [OneTimeSetUp]
        public void Initialize()
        {
            host = new EngineWrapperNUnit();
            host.StartEngine();
            solutionRoot = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..");
            host.Engine.InitExternalLibraries(Path.Combine(solutionRoot, "oscript-library", "src"), null);
        }

        [Test]
        public void RunEngineTests()
        {
            var testRunnerPath = Path.Combine(solutionRoot, "tests", "testrunner.os");

            Assert.IsTrue(File.Exists(testRunnerPath),
                "Запускатель тестов отсутствует по пути " + testRunnerPath);

            var result = host.RunTestScriptFromPath(testRunnerPath, "-runall " + new FileInfo(testRunnerPath).Directory.FullName);

            if (result == TEST_STATE_FAILED)
            {
                Assert.Fail("Есть непройденные тесты!");
            }
        }
    }
}
