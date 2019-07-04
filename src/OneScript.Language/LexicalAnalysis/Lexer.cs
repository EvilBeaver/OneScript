namespace OneScript.Language.LexicalAnalysis
{
    public class Lexer : FullSourceLexer
    {
        public override Lexem NextLexem()
        {
            Lexem lex;
            while((lex = base.NextLexem()).Type == LexemType.Comment)
                ; // skip

            return lex;
        }
    }
}