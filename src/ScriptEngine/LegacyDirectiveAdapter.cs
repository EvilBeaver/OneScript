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

        public void OnModuleEnter(IAstBuilder nodeBuilder, ILexer lexemStream)
        {
        }

        public void OnModuleLeave(ILexer lexemStream)
        {
        }

        public BslSyntaxNode HandleDirective(BslSyntaxNode parent, ILexer lexemStream, ref Lexem lastExtractedLexem)
        {
            var directive = lastExtractedLexem.Content;
            lexemStream.ReadToLineEnd();
            var content = lexemStream.Iterator.GetContents();

            var handled = RealResolver.Resolve(directive, content, parent?.Kind == NodeKind.Module);
            
            lastExtractedLexem = lexemStream.NextLexem();

            return handled ? parent : default;
        }
    }
}