/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;

namespace OneScriptDocumenter.Cli
{
    public class MarkdownWriter : IDisposable
    {
        private const int IndentSize = 4;
        
        private readonly TextWriter _output;
        private int _currentListLevel = 0;

        public MarkdownWriter(TextWriter output)
        {
            _output = output;
        }

        public static MarkdownWriter OpenFile(string filePath)
        {
            return new MarkdownWriter(new StreamWriter(filePath));
        }

        public void Raw(string rawData)
        {
            _output.Write(rawData);
        }
        
        public void Header1(string text)
        {
            _output.WriteLine();
            _output.WriteLine($"# {text}");
            _output.WriteLine();
        }
        
        public void Header2(string text)
        {
            _output.WriteLine();
            _output.WriteLine($"## {text}");
            _output.WriteLine();
        }
        
        public void Header3(string text)
        {
            _output.WriteLine();
            _output.WriteLine($"### {text}");
            _output.WriteLine();
        }
        
        public void Header4(string text)
        {
            _output.WriteLine();
            _output.WriteLine($"#### {text}");
            _output.WriteLine();
        }

        public void Paragraph(string text)
        {
            if (text == null)
                return;
            
            _output.WriteLine();
            _output.WriteLine(text);
            _output.WriteLine();
        }

        public void BeginList(bool resetLevel = false)
        {
            if (resetLevel)
                _currentListLevel = 0;
            
            _currentListLevel++;
        }
        
        public void EndList()
        {
            _currentListLevel--;
            if (_currentListLevel < 0)
                _currentListLevel = 0;
        }

        public void ListItem(string text)
        {
            if (_currentListLevel == 0)
                BeginList();
            
            var indent = new string(' ', (_currentListLevel - 1) * IndentSize);
            
            _output.Write(indent);
            _output.WriteLine("* " + text);
        }

        public void Dispose()
        {
            _output.Flush();
            _output?.Dispose();
        }

        public void WriteCode(string code)
        {
            _output.WriteLine();
            _output.WriteLine("```bsl");
            _output.WriteLine(code);
            _output.WriteLine("```");
            _output.WriteLine();
        }
    }
}