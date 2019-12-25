/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using ScriptEngine.Environment;

namespace ScriptEngine.Compiler
{
    public class DirectiveMultiResolver : List<IDirectiveResolver>, IDirectiveResolver
    {

        public ICodeSource Source
        {
            get
            {
                foreach (var resolver in this)
                {
                    var result = resolver.Source;
                    if (result != null)
                    {
                        return result;
                    }
                }

                return null;
            }
            set
            {
                foreach (var resolver in this)
                {
                    resolver.Source = value;
                }
            }
        }

        public DirectiveMultiResolver ()
        {
        }

        public DirectiveMultiResolver(IEnumerable<IDirectiveResolver> resolvers)
        {
            this.AddRange(resolvers);
        }

        public bool Resolve(string directive, string value, bool codeEntered)
        {
            foreach (var resolver in this)
            {
                if (resolver.Resolve(directive, value, codeEntered))
                    return true;
            }
            return false;
        }
    }
}

