using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using ScriptEngine.Compiler;
using ScriptEngine.Environment;

namespace ScriptEngine
{
    //Resharper disable CS0612
    public class LegacyDirectiveAdapter : IDirectiveResolver, IDirectiveHandler
    {
        public IDirectiveResolver RealResolver { get; }

        public LegacyDirectiveAdapter(IDirectiveResolver realResolver)
        {
            RealResolver = realResolver;
        }

        public ICodeSource Source
        {
            get => RealResolver.Source;
            set => RealResolver.Source = value;
        }
        
        public bool Resolve(string directive, string value, bool codeEntered)
        {
            return RealResolver.Resolve(directive, value, codeEntered);
        }

        public void OnModuleEnter(ParserContext context)
        {
        }

        public void OnModuleLeave(ParserContext context)
        {
        }

        public bool HandleDirective(ParserContext context)
        {
            var directive = context.LastExtractedLexem.Content;
            var lexemStream = context.Lexer;
            lexemStream.ReadToLineEnd();
            var content = lexemStream.Iterator.GetContents();

            var handled = RealResolver.Resolve(directive, content, context.NodeContext.Peek()?.Kind == NodeKind.Module);
            
            context.LastExtractedLexem = lexemStream.NextLexem();

            return handled;
        }
    }
}