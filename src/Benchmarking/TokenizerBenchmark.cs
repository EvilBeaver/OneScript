using System.IO;
using System.Reflection;
using System.Text;
using BenchmarkDotNet.Attributes;
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;

namespace Benchmarking
{
    public class TokenizerBenchmark
    {
        [Params(0, 1, 2)]
        public int useTrie;

        private string GetCode(string resourceName)
        {
            using(var data = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(data, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            if (useTrie == 1)
            {
                LanguageDef._stringToToken = new IdentifiersTrie<Token>();
            }
            if (useTrie == 2)
            {
                LanguageDef._stringToToken = new LexemTrie<Token>();
            }
            LanguageDef.init();
        }

        [Benchmark]
        [Arguments("bigModule.bsl")]
        [Arguments("Module.bsl")]
        public void Tokenizer(string resourceName)
        {
            var lex = new Lexer();
            lex.Code = GetCode("Benchmarking."+resourceName);
            Lexem lexem;
            do
            {
                lexem = lex.NextLexem();
            } while (lexem.Type != LexemType.EndOfText);
        }
    }
}