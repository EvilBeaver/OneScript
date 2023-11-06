/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Moq;
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using ScriptEngine;
using Xunit;

namespace OneScript.Core.Tests
{
    public class ModuleSerializationTest
    {
        [Fact]
        public void TestThatModuleInfoIsSerialized()
        {
            var mi = new ModuleImage
            {
                ModuleInfo = new ModuleInformation
                {
                    Origin = "AAA",
                    ModuleName = "BBB",
                    CodeIndexer = Mock.Of<ISourceCodeIndexer>()
                }
            };

            var formatter = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                formatter.Serialize(ms, mi);
                ms.Position = 0;

                var obj = (ModuleImage)formatter.Deserialize(ms);
                Assert.Equal("AAA", obj.ModuleInfo.Origin);
                Assert.Equal("BBB", obj.ModuleInfo.ModuleName);
                Assert.Null(obj.ModuleInfo.CodeIndexer);
            }
        }
    }
}
