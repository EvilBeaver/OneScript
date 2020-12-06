/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;

namespace OneScript.Language.SyntaxAnalysis
{
    public class PreprocessorHandlerChain : IDirectiveHandler
    {
        private readonly IList<IDirectiveHandler> _handlers = new List<IDirectiveHandler>();
        
        public void Add(IDirectiveHandler handler)
        {
            _handlers.Add(handler);
        }
        
        public void Remove(IDirectiveHandler handler)
        {
            _handlers.Remove(handler);
        }

        void IDirectiveHandler.OnModuleEnter(ILexer lexemStream)
        {
            foreach (var handler in _handlers)
            {
                handler.OnModuleEnter(lexemStream);
            }
        }

        void IDirectiveHandler.OnModuleLeave(ILexer lexemStream)
        {
            foreach (var handler in _handlers)
            {
                handler.OnModuleLeave(lexemStream);
            }
        }

        BslSyntaxNode IDirectiveHandler.HandleDirective(BslSyntaxNode parent, ILexer lexemStream, ref Lexem lastExtractedLexem)
        {
            BslSyntaxNode result = default;
            foreach (var handler in _handlers)
            {
                result = handler.HandleDirective(parent, lexemStream, ref lastExtractedLexem);
                if (result != default)
                    break;
            }

            return result;
        }
    }
}