/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Moq;
using OneScriptDocumenter.Secondary;
using Xunit;

namespace DocumenterTests
{
    public class XmlDocConversionTest
    {
        [Fact]
        public void TestConversionOfTextBlock()
        {
            using var xmlStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("DocumenterTests.Resources.SimpleMarkup.xml");
            Assert.NotNull(xmlStream);

            var doc = XDocument.Load(xmlStream);
            var exampleNode = doc.Root.Elements("example").First();

            var converter = new XmlDocConverter(Mock.Of<IReferenceResolver>());
            var text = converter.ConvertTextBlock(exampleNode);
            var expected = 
@"Для Каждого Переменная Из ПеременныеСреды() Цикл
    Сообщить(Переменная.Ключ + "" = "" + Переменная.Значение);
КонецЦикла;";
            
            Assert.Equal(expected, text);
        }

        [Fact]
        public void TestConversionOfInlineCode()
        {
            using var xmlStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("DocumenterTests.Resources.SimpleMarkup.xml");
            Assert.NotNull(xmlStream);

            var doc = XDocument.Load(xmlStream);
            var exampleNode = doc.Root.Elements("returns").First();

            var converter = new XmlDocConverter(Mock.Of<IReferenceResolver>());
            var text = converter.ConvertTextBlock(exampleNode);
            var expected = "`Соответствие` соответствие Имя-Значение переменных среды";

            Assert.Equal(expected, text);
        }

        [Fact]
        public void CanExtractLinkFromXmlDoc()
        {
            var resolver = new Mock<IReferenceResolver>();
            resolver.Setup(r => r.Resolve(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((linkTarget, linkText) => $"[{linkText}]({linkTarget})");

            var converter = new XmlDocConverter(resolver.Object);
            var elementWithLink = new XElement("summary",
                new XText("Это будет "),
                new XElement("see", new XAttribute("cref", "referenceForSomeType"),
                    new XText("гиперссылка")),
                new XText(" на другой документ"));

            var text = converter.ConvertTextBlock(elementWithLink);
            Assert.Equal("Это будет [гиперссылка](referenceForSomeType) на другой документ", text);
            resolver.Verify(r =>
                r.Resolve(It.Is<string>(a => a == "referenceForSomeType"), It.Is<string>(a => a == "гиперссылка")));
        }
    }
}