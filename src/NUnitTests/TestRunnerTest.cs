using System;
using System.IO;
using NUnit.Framework;

namespace NUnitTests
{
    [TestFixture]
    public class UnitTestWrapper
    {
        const int TEST_STATE_FAILED = 3;

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
            String testRunnerPath = Path.Combine(solutionRoot, "tests", "testrunner.os");

            Assert.IsTrue(File.Exists(testRunnerPath),
                "Запускатель тестов отсутствует по пути " + testRunnerPath);

            var result = host.RunTestScriptFromPath(testRunnerPath, "-runall " + new System.IO.FileInfo(testRunnerPath).Directory.FullName);

            if (result == TEST_STATE_FAILED)
            {
                NUnit.Framework.Assert.Fail("Есть непройденные тесты!");
            }
        }
    }
}
