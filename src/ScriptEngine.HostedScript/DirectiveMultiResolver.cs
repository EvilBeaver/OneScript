using System;
using System.Collections.Generic;
using ScriptEngine.Environment;

namespace ScriptEngine.HostedScript
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

