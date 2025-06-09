/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using CommandLine;
using OneScriptDocumenter.Cli;
using OneScriptDocumenter.Model;
using OneScriptDocumenter.Primary;
using OneScriptDocumenter.Secondary;
using MarkdownGenerator = OneScriptDocumenter.Cli.MarkdownGenerator;

namespace OneScriptDocumenter
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var parserResult = Parser.Default.ParseArguments<GeneratorOptions>(args);

            switch (parserResult.Tag)
            {
                case ParserResultType.Parsed:
                    try
                    {
                        return ProcessOptions(parserResult.Value);
                    }
                    catch(Exception e)
                    {
                        ConsoleLogger.Error(e.ToString());
                        return 1;
                    }
                default:
                    return 1;
            }
        }

        private static int ProcessOptions(GeneratorOptions options)
        {
            if (string.IsNullOrEmpty(options.JsonFile) && string.IsNullOrEmpty(options.MarkdownDir))
            {
                ConsoleLogger.Error("Must have either --json or --markdown options");
                return 1;
            }

            var primaryGenerator = new PrimaryDocsCollector();
            var primaryDocs = primaryGenerator.Collect(options.AssemblyFiles.ToList());

            var refFactory = new ReferenceFactory();
            var referenceResolver = new MarkdownReferenceResolver(primaryDocs.ReferenceCollector, refFactory);
            
            var tocFile = options.TocFile;
            var docBuilder = new DocumentationModelBuilder(primaryDocs, referenceResolver, tocFile);
            var secondaryDocs = docBuilder.Build();

            if (!string.IsNullOrEmpty(options.JsonFile))
            {
                WriteJson(secondaryDocs, options.JsonFile);
            }
            
            if (!string.IsNullOrEmpty(options.MarkdownDir))
            {
                WriteMarkdown(secondaryDocs, options.MarkdownDir);
            }
            
            ConsoleLogger.Info("Generation completed");

            return 0;
        }

        private static void WriteMarkdown(DocumentationModel documentation, string outputDir)
        {
            var refFactory = new ReferenceFactory();
            var mdGenerator = new MarkdownGenerator(documentation, refFactory);
            mdGenerator.Write(outputDir);
        }

        private static void WriteJson(DocumentationModel documentation, string outputFile)
        {
            var jsonWriter = new JsonGenerator(documentation);
            jsonWriter.WriteFile(outputFile);
        }
    }
}