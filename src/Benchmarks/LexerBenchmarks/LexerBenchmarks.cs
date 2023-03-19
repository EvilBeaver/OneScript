/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Text;
using BenchmarkDotNet.Attributes;
using OneScript.Language.LexicalAnalysis;
using OneScript.Sources;

namespace OneScript.Benchmarks;

public class LexerBenchmarks
{
    // specify code to feed lexer
    private const string sourcePath = @"c:\Users\ovsia\Documents\For videos\unf_scr\Catalogs\Банки\Ext\";

    private IEnumerable<string> _files;
    private ILexer _dynamicLexer;
    
    [GlobalSetup]
    public void Setup()
    {
        _files = Directory.EnumerateFiles(sourcePath, "*.bsl", SearchOption.AllDirectories);

        var builder = new LexerBuilder();
        builder
            .DetectWords()
            .DetectOperators()
            .DetectComments()
            .DetectNumbers()
            .DetectStrings()
            .DetectDates()
            .DetectSemicolons()
            .DetectAnnotations()
            .DetectTernaryOperators()
            .DetectPreprocessorDirectives();

        _dynamicLexer = builder.Build();
    }
    
    
    [Benchmark]
    public void BenchmarkDefaultLexer()
    {
        LexAllFiles(new FullSourceLexer());
    }
    
    [Benchmark]
    public void BenchmarkDynamicLexer()
    {
        LexAllFiles(_dynamicLexer);
    }

    private void LexAllFiles(ILexer lexer)
    {
        foreach (var file in _files)
        {
            var content = File.ReadAllText(file, Encoding.UTF8);
            lexer.Iterator = SourceCodeBuilder.Create()
                .FromString(content)
                .Build()
                .CreateIterator();

            while (lexer.NextLexem().Type != LexemType.EndOfText)
            {
            }
        }
    }
}