/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Linq;

namespace OneScriptDocumenter
{
    static class Program
    {
        static int Main(string[] args)
        {
            int retCode = 0;

            try
            {
                if (args.Length > 0 && args[0] == "markdown")
                {
                    retCode = GenerateMarkdown(args);
                }
                else if (args.Length > 0 && args[0] == "html")
                {
                    retCode = GenerateHtml(args);
                }
                else if (args.Length > 0 && args[0] == "json")
                {
                    retCode = GenerateJSON(args);
                }
                else
                {
                    retCode = GenerateXml(args);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                retCode = 128;
            }

            if (System.Diagnostics.Debugger.IsAttached)
                Console.ReadKey();

            return retCode;

        }

        private static int GenerateHtml(string[] args)
        {
            var cmdLineArgs = new CommandLineArgs(args);
            cmdLineArgs.Next(); // пропуск ключевого слова

            var xmlDoc = cmdLineArgs.Next();
            if (xmlDoc == null)
            {
                ShowUsage();
                return 1;
            }

            var outputDir = cmdLineArgs.Next();
            if (outputDir == null)
                outputDir = Directory.GetCurrentDirectory();

            var baseUrl = cmdLineArgs.Next();

            var inputDir = Path.Combine(Environment.GetEnvironmentVariable("TMP"), Path.GetRandomFileName());
            Directory.CreateDirectory(inputDir);

            CreateDocumentation(xmlDoc, inputDir, baseUrl);

            var files = Directory.EnumerateFiles(inputDir, "*.md", SearchOption.AllDirectories)
                .Select(x => new { FullPath = x, RelativePath = x.Substring(inputDir.Length + 1) });

            Directory.CreateDirectory(outputDir);
            var mdGen = new MarkdownGen();
            mdGen.ExtraMode = true;
            mdGen.UrlBaseLocation = "stdlib";

            var layout = ReadHTMLLayout();

            foreach (var file in files)
            {
                Console.WriteLine("Processing {0}", file.RelativePath);
                using (var inputFile = new StreamReader(file.FullPath))
                {
                    var content = inputFile.ReadToEnd();
                    var outputFile = Path.Combine(outputDir, file.RelativePath.Substring(0, file.RelativePath.Length - 2) + "htm");

                    var html = mdGen.Transform(content);
                    Directory.CreateDirectory(Path.GetDirectoryName(outputFile));

                    using (var outStream = new StreamWriter(outputFile, false, Encoding.UTF8))
                    {
                        var full_html = layout.Replace("$title$", Path.GetFileNameWithoutExtension(outputFile))
                            .Replace("$body$", html);
                        outStream.Write(full_html);
                    }
                }
            }

            Directory.Delete(inputDir, true);
            Console.WriteLine("Done");
            return 0;
        }

        private static string ReadHTMLLayout()
        {
            using (var sr = new StreamReader(ExtFiles.Get("html_layout.htm")))
            {
                return sr.ReadToEnd();
            }
        }

        private static int GenerateJSON(string[] args)
        {
            var cmdLineArgs = new CommandLineArgs(args);
            cmdLineArgs.Next(); // пропуск ключевого слова

            var outputFile = cmdLineArgs.Next();
            if (outputFile == null)
            {
                ShowUsage();
                return 1;
            }

            List<string> assemblies = new List<string>();

            while (true)
            {
                var arg = cmdLineArgs.Next();
                if (arg == null)
                    break;

                assemblies.Add(arg);
            }

            if (assemblies.Count == 0)
            {
                ShowUsage();
                return 1;
            }

            return CreateDocumentationJSON(outputFile, assemblies);

        }

        private static int GenerateXml(string[] args)
        {
            var cmdLineArgs = new CommandLineArgs(args);

            var outputFile = cmdLineArgs.Next();
            if (outputFile == null)
            {
                ShowUsage();
                return 1;
            }

            List<string> assemblies = new List<string>();

            while (true)
            {
                var arg = cmdLineArgs.Next();
                if (arg == null)
                    break;

                assemblies.Add(arg);
            }

            if (assemblies.Count == 0)
            {
                ShowUsage();
                return 1;
            }

            return CreateDocumentation(outputFile, assemblies);

        }

        private static int GenerateMarkdown(string[] args)
        {
            var cmdLineArgs = new CommandLineArgs(args);
            cmdLineArgs.Next(); // пропуск ключевого слова

            var xmlDoc = cmdLineArgs.Next();
            if (xmlDoc == null)
            {
                ShowUsage();
                return 1;
            }

            var outputDir = cmdLineArgs.Next();
            if (outputDir == null)
                outputDir = Directory.GetCurrentDirectory();

            var baseUrl = cmdLineArgs.Next();

            return CreateDocumentation(xmlDoc, outputDir, baseUrl);

        }

        private static int CreateDocumentation(string outputFile, List<string> assemblies)
        {
            var documenter = new Documenter();
            var doc = documenter.CreateDocumentation(assemblies);

            doc.Save(outputFile);

            return 0;
        }

        private static int CreateDocumentationJSON(string outputFile, List<string> assemblies)
        {
            var documenter = new Documenter();
            documenter.CreateDocumentationJSON(outputFile, assemblies);

            return 0;
        }

        private static int CreateDocumentation(string xmlDocPath, string pathOutput, string baseUrl)
        {
            XDocument doc;
            using (var fs = new FileStream(xmlDocPath, FileMode.Open, FileAccess.Read))
            {
                doc = XDocument.Load(fs);
            }

            string docContent = doc.ToString();

            XslCompiledTransform xslt = new XslCompiledTransform();
            xslt.Load(ExtFiles.Get("markdown.xslt"));
            XPathDocument xpathdocument = new XPathDocument(new StringReader(docContent));

            var stream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);

            xslt.Transform(xpathdocument, null, writer, null);

            stream.Position = 0;
            XDocument xdoc = XDocument.Load(stream);
            writer.Close();

            if (!Directory.Exists(pathOutput))
                Directory.CreateDirectory(pathOutput);

            var contentStdlibPath = Path.Combine(pathOutput, "stdlib");
            Directory.CreateDirectory(contentStdlibPath);

            var tocBuilder = new StringBuilder();
            var knownNodes = new HashSet<string>();
            if (baseUrl == null)
                baseUrl = "";

            using (var layout = new StreamReader(ExtFiles.Get("toc_layout.md")))
            {
                var content = layout.ReadToEnd();
                tocBuilder.Append(content);
                tocBuilder.Replace("$base_url$", baseUrl);
                var matches = Regex.Matches(content, @"(?=\S)\[(.*)\]\S");
                foreach (Match match in matches)
                {
                    var uri = match.Groups[1].Value;
                    knownNodes.Add(uri);
                }
            }

            using (var tocWriter = new StreamWriter(Path.Combine(pathOutput, "stdlib.md")))
            {
                tocWriter.Write(tocBuilder.ToString());
                tocBuilder.Clear();

                foreach (var fileNode in xdoc.Root.Elements("document"))
                {
                    string name = fileNode.Attribute("href").Value.Replace(".md", "");
                    string link = name.Replace(" ", "%20");

                    string path = Path.Combine(contentStdlibPath, fileNode.Attribute("href").Value);
                    var file = new FileStream(path, FileMode.Create);
                    using (var fileWriter = new StreamWriter(file))
                    {
                        fileWriter.Write(fileNode.Value);
                    }

                    if (!knownNodes.Contains(name))
                        tocWriter.WriteLine("* [{0}]({1}/{2})", name, baseUrl, link);

                }
            }

            return 0;
        }

        static void ShowUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("OneScriptDocumenter.exe <output-file> <path-to-dll> [<path-to-dll>...]");
            Console.WriteLine("OneScriptDocumenter.exe json <output-file> <path-to-dll> [<path-to-dll>...]");
            Console.WriteLine("OneScriptDocumenter.exe markdown <path-to-xml> <output-dir> [baseurl]");
            Console.WriteLine("OneScriptDocumenter.exe html <path-to-xml> <output-dir> [baseurl]");
        }

    }
}
