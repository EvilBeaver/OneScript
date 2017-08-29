using NUnit.Framework;
using System.IO;

namespace oscript
{
    [TestFixture]
    public class oscriptTests
    {
        [Test, Category("NoArgs")]
        public void ShowUsageBehaviourExecutes()
        {
            var args = new string[0];
            Program.Main(args);
        }

        [Test, Category("Make")]
        public void BasicMakeBehaviourWorks()
        {
            var tempFolder = Path.GetTempPath();
            const string exeName = "test.exe";

            var osFullPath = Path.GetFullPath("test.os");
            var exeFullPath = Path.Combine(tempFolder, exeName);

            if (File.Exists(exeFullPath))
            {
                File.Delete(exeFullPath);
            }

            var args = new[]
            {
                "-make",
                osFullPath,
                exeFullPath
            };

            Program.Main(args);

            var exeExists = File.Exists(exeFullPath);
            Assert.IsTrue(exeExists);
        }
    }
}