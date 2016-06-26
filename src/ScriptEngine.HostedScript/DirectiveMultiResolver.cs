using System;
using System.Collections.Generic;
using ScriptEngine.Environment;

namespace ScriptEngine.HostedScript
{
    public class DirectiveMultiResolver : List<IDirectiveResolver>, IDirectiveResolver
    {

        public ICodeSource Source { get; set; }

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

