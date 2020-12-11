/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
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
            var content = lexemStream.Iterator.ReadToLineEnd();

            var handled = RealResolver.Resolve(directive, content, context.NodeContext.Peek()?.Kind != NodeKind.Module);
            
            context.LastExtractedLexem = lexemStream.NextLexem();

            return handled;
        }
    }
}