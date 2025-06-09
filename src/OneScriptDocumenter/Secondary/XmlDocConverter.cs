/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace OneScriptDocumenter.Secondary
{
    public class XmlDocConverter
    {
        private readonly IReferenceResolver _referenceResolver;

        public XmlDocConverter(IReferenceResolver referenceResolver)
        {
            _referenceResolver = referenceResolver;
        }

        public string ConvertTextBlock(XElement? textBlock, bool resolveLinks = true)
        {
            if (textBlock == null)
                return "";
            
            var markdownContent = new StringBuilder();

            VisitNodes(markdownContent, textBlock);

            return markdownContent.ToString();
        }

        public IReadOnlyCollection<string> ConvertSeeAlsoList(XElement docs)
        {
            if (docs == null)
                return Array.Empty<string>();
            
            var seeAlso = docs.Elements("seealso").ToList();
            if (seeAlso.Count == 0)
                return Array.Empty<string>();

            return seeAlso.Select(node =>
            {
                var markdownContent = new StringBuilder();
                ProcessLinkElement(node, markdownContent);
                return markdownContent.ToString();
            }).ToList();
        }

        private void ProcessLinkElement(XElement node, StringBuilder markdownContent)
        {
            var targetAttribute = node.Attribute("cref");
            if (targetAttribute == null)
            {
                // Не будем специально обрабатывать этот элемент
                VisitNodes(markdownContent, node);
            }
            else
            {
                var referenceTarget = targetAttribute.Value;
                var referenceText = node.Value;
                var linkCode = _referenceResolver.Resolve(referenceTarget, referenceText);
                markdownContent.Append(linkCode);
            }
        }

        private void VisitNodes(StringBuilder markdownContent, XElement textBlock)
        {
            bool trimFirstLine = true;
            foreach (var node in textBlock.Nodes())
            {
                switch (node.NodeType)
                {
                    case XmlNodeType.Text:
                        ProcessTextBlock(markdownContent, (XText)node, trimFirstLine);
                        break;
                    case XmlNodeType.Element:
                        trimFirstLine = false;
                        ProcessInlineElement(markdownContent, (XElement)node);
                        break;
                }
            }
        }

        private void ProcessInlineElement(StringBuilder markdownContent, XElement node)
        {
            if (node.Name == "see")
            {
                ProcessLinkElement(node, markdownContent);
            }
            else if (node.Name == "c")
            {
                markdownContent.Append('`');
                markdownContent.Append(node.Value);
                markdownContent.Append('`');
            }
            else if (node.Name == "para")
            {
                markdownContent.AppendLine();
                VisitNodes(markdownContent, node);
                markdownContent.AppendLine();
            }
        }

        private void ProcessTextBlock(StringBuilder sb, XText textNode, bool trimFirstLine)
        {
            int indentOfFirstLine = 0;
            bool lineWritten = false;
            bool firstLineFound = false;

            using var sr = new StringReader(textNode.Value);
            
            string line = null;
            do
            {
                line = sr.ReadLine();
                if (line != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        lineWritten = firstLineFound;
                        continue;
                    }

                    if (lineWritten)
                    {
                        sb.AppendLine();
                    }

                    if (firstLineFound)
                    {
                        sb.Append(line.Substring(indentOfFirstLine));
                    }
                    else
                    {
                        firstLineFound = true;
                        indentOfFirstLine = CalculateIndent(line);
                        sb.Append(trimFirstLine ? line.TrimStart() : line);
                    }
                    
                    lineWritten = true;
                } 
            } while (line != null);
        }

        private int CalculateIndent(string line)
        {
            for (int i = 0; i < line.Length; i++)
            {
                if (!char.IsWhiteSpace(line[i]))
                {
                    return i;
                }
            }

            return 0;
        }
    }
}