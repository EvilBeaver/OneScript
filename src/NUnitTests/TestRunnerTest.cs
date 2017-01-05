using System;

using NUnit.Framework;

namespace NUnitTests
{
    [TestFixture]
    public class UnitTestWrapper
    {
        const int TEST_STATE_FAILED = 3;

        private EngineWrapperNUnit host;

        [OneTimeSetUp]
        public void Initialize()
        {
            host = new EngineWrapperNUnit();
            host.StartEngine();
        }

        [Test]
        public void RunEngineTests()
        {
            host = new EngineWrapperNUnit();
            host.StartEngine();

            String testRunnerPath = System.IO.Path.Combine(
                NUnit.Framework.TestContext.CurrentContext.TestDirectory, "..","..","..","..",
                "tests", "testrunner.os"
              );

            NUnit.Framework.Assert.IsTrue(System.IO.File.Exists(testRunnerPath),
                "Запускатель тестов отсутствует по пути " + testRunnerPath);



            var result = host.RunTestScriptFromPath(testRunnerPath, "-runall " + new System.IO.FileInfo(testRunnerPath).Directory.FullName);

            if (result == TEST_STATE_FAILED)
            {
                NUnit.Framework.Assert.Fail("Есть непройденные тесты!");
            }
        }
    }
}
