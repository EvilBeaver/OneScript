/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace OneScript.Language.SyntaxAnalysis
{
    public class PreprocessorHandlersFactory
    {
        private readonly List<Func<ParserOptions, IDirectiveHandler>> _factories;

        public PreprocessorHandlersFactory()
        {
            _factories = new List<Func<ParserOptions, IDirectiveHandler>>();
        }

        public void Add(Func<ParserOptions, IDirectiveHandler> activator)
        {
            _factories.Add(activator);
        }

        public PreprocessorHandlers Create(ParserOptions options)
        {
            var handlers = _factories.Select(x => x(options));
            return new PreprocessorHandlers(handlers);
        }
    }
}