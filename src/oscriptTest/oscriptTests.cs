using NUnit.Framework;
using System;
using System.IO;

namespace oscript
{
    [TestFixture]
    public class oscriptTests
    {
        private string _osFullPath;

        [SetUp]
        public void SetUp()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            _osFullPath = Path.Combine(path, "NearlyEmptyScript.os");
        }

        [Test, Category("NoArgs")]
        public void ShowUsageBehaviourExecutes()
        {
            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);

                var args = new string[]
                {
                };
                Program.Main(args);
                var expected = "  Runs as CGI application under HTTP-server (Apache/Nginx/IIS/etc...)";
                var result = GetLastOutput(sw.ToString());
                Assert.AreEqual(expected, result);
            }
        }

        [Test, Category("Make")]
        public void BasicMakeBehaviourWorks()
        {
            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);

                var tempFolder = Path.GetTempPath();
                const string exeName = "test.exe";

                var exeFullPath = Path.Combine(tempFolder, exeName);

                if (File.Exists(exeFullPath))
                {
                    File.Delete(exeFullPath);
                }

                var args = new[]
                {
                    "-make",
                    _osFullPath,
                    exeFullPath
                };

                Program.Main(args);

                var exeExists = File.Exists(exeFullPath);
                var expected = "Make completed";
                var result = GetLastOutput(sw.ToString());
                Assert.AreEqual(expected, result);
                Assert.IsTrue(exeExists);
            }
        }

        [Test, Category("Compile")]
        public void BasicCompileBehaviourWorks()
        {
            var args = new[]
            {
                "-compile",
                _osFullPath
            };

            Program.Main(args);
        }

        [Test, Category("Script execution")]
        public void FileNotFoundReturnsError()
        {
            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);

                var args = new[]
                {
                    "nonsense.os"
                };

                Program.Main(args);
                const string expected = "Script file is not found \'nonsense.os\'";
                var result = GetLastOutput(sw.ToString());
                Assert.AreEqual(expected, result);
            }
        }

        [Test, Category("Script execution")]
        public void SimpleScriptExecutes()
        {
            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);

                var args = new[]
                {
                    _osFullPath
                };

                Program.Main(args);
                var result = sw.ToString();
                Assert.IsEmpty(result);
            }
        }

        private static string GetLastOutput(string text)
        {
            var array = text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            return array[array.Length - 1];
        }
    }
}