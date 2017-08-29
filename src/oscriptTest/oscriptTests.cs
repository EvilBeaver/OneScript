using NUnit.Framework;
using System;
using System.IO;

namespace oscript
{
    [TestFixture]
    public class oscriptTests
    {
        private string _simpleScriptFullPath;
        private string _uncompilableScriptFullPath;

        [SetUp]
        public void SetUp()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            _simpleScriptFullPath = Path.Combine(path, "simpleScript.os");
            _uncompilableScriptFullPath = Path.Combine(path, "uncompilableScript.os");
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
                    _simpleScriptFullPath,
                    exeFullPath
                };

                Program.Main(args);

                var exeExists = File.Exists(exeFullPath);
                var expected = "Make completed";
                var result = GetLastOutput(sw.ToString());
                Assert.AreEqual(expected, result);
                Assert.IsTrue(exeExists, "Exptected file: " + exeFullPath);
            }
        }

        [Test, Category("Make")]
        public void BasicMakeBehaviourNonsenseFileReturnsError()
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
                    "nonsense.os",
                    exeFullPath
                };

                Program.Main(args);

                var exeExists = File.Exists(exeFullPath);
                var unexpected = "Make completed";
                var result = GetLastOutput(sw.ToString());
                Assert.AreNotEqual(unexpected, result);
                //Assert.IsFalse(exeExists); //TODO Когда-нибудь -make перестанет создавать файл в случае провала.
            }
        }

        [Test, Category("Compile")]
        public void BasicCompileBehaviourWorks()
        {
            var args = new[]
            {
                "-compile",
                _simpleScriptFullPath
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
                    _simpleScriptFullPath
                };

                Program.Main(args);
                var result = sw.ToString();
                Assert.IsEmpty(result);
            }
        }

        [Test, Category("Check")]
        public void UncompilableScriptFailsCheck()
        {
            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);

                var args = new[]
                {
                    "-check",
                    _uncompilableScriptFullPath
                };

                Program.Main(args);
                var result = sw.ToString();
                var unexpected = "No errors.\r\n";
                Assert.AreNotEqual(unexpected, result);
            }
        }

        [Test, Category("Check")]
        public void SimpleScriptChecksOut()
        {
            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);

                var args = new[]
                {
                    "-check",
                    _simpleScriptFullPath
                };

                Program.Main(args);
                var result = sw.ToString();
                var expected = "No errors.\r\n";
                Assert.AreEqual(expected, result);
            }
        }

        private static string GetLastOutput(string text)
        {
            var array = text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            return array[array.Length - 1];
        }
    }
}