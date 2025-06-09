/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using CommandLine;

namespace OneScriptDocumenter.Cli
{
    public class GeneratorOptions
    {
        [Value(0, Min = 1, HelpText = "Список dll файлов по которым создается документация")]
        public IEnumerable<string> AssemblyFiles { get; set; }
        
        [Option('j', "json", HelpText = "Генерировать документацию в виде json-файла")]
        public string JsonFile { get; set; }
        
        [Option('m', "markdown", HelpText = "Выходной каталог документации в виде набора md-файлов")]
        public string MarkdownDir { get; set; }
        
        [Option('t', "toc", HelpText = "Файл оглавления")]
        public string TocFile { get; set; }
    }
}