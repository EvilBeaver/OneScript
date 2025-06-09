/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.IO;
using OneScriptDocumenter.Cli;
using Xunit;

namespace DocumenterTests
{
    public class MarkdownWriterTests
    {
        [Fact]
        public void TestListIndentZeroIndent()
        {
            var stringWriter = new StringWriter();
            var mdWriter = new MarkdownWriter(stringWriter);
            
            mdWriter.BeginList();
            mdWriter.ListItem("One");
            mdWriter.ListItem("Two");
            mdWriter.ListItem("Three");
            mdWriter.EndList();
            
            stringWriter.Flush();

            var result = stringWriter.ToString();
            
            Assert.Equal("* One\r\n" +
                         "* Two\r\n" +
                         "* Three\r\n", result);

        }
        
        [Fact]
        public void TestListIndentTwoLevelList()
        {
            var stringWriter = new StringWriter();
            var mdWriter = new MarkdownWriter(stringWriter);
            
            mdWriter.BeginList();
            mdWriter.ListItem("One");
            mdWriter.ListItem("Two");
            mdWriter.BeginList();
            mdWriter.ListItem("Three");
            mdWriter.ListItem("Four");
            mdWriter.EndList();
            mdWriter.ListItem("Five");
            mdWriter.EndList();
            
            stringWriter.Flush();

            var result = stringWriter.ToString();
            
            Assert.Equal("* One\r\n" +
                         "* Two\r\n" +
                         "    * Three\r\n" +
                         "    * Four\r\n" +
                         "* Five\r\n", result);

        }
    }
}