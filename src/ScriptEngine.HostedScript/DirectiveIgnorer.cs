/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using ScriptEngine.Environment;

namespace ScriptEngine.HostedScript
{
    public class DirectiveIgnorer : List<string>, IDirectiveResolver
    {

        public ICodeSource Source {
            get { return null;  }
            set { }
        }

        public DirectiveIgnorer ()
        {
        }

        public void Add(string directive, string alias)
        {
            Add(directive);
            Add(alias);
        }

        public bool Resolve(string directive, string value, bool codeEntered)
        {
            if (FindIndex(word => word.Equals(directive, StringComparison.InvariantCultureIgnoreCase)) == -1)
                return false;
            return true;
        }
    }
}

