/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using OneScript.Language.LexicalAnalysis;
using ScriptEngine;
using ScriptEngine.Environment;

namespace NUnitTests
{
    [TestFixture]
    public class ModuleSerializationTest
    {
        [Test]
        public void TestThatModuleInfoIsSerialized()
        {
            var mi = new ModuleImage
            {
                ModuleInfo = new ModuleInformation
                {
                    Origin = "AAA",
                    ModuleName = "BBB",
                    CodeIndexer = new SourceCodeIterator()
                }
            };

            var formatter = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                formatter.Serialize(ms, mi);
                ms.Position = 0;

                var obj = (ModuleImage)formatter.Deserialize(ms);
                Assert.That(obj.ModuleInfo.Origin, Is.EqualTo("AAA"));
                Assert.That(obj.ModuleInfo.ModuleName, Is.EqualTo("BBB"));
                Assert.That(obj.ModuleInfo.CodeIndexer, Is.Null);
            }
        }
    }
}
