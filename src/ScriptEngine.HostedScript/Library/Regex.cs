/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.Text.RegularExpressions;

namespace ScriptEngine.HostedScript.Library
{
    [ContextClass("РегулярноеВыражение", "Regex")]
    class RegExpImpl : AutoContext<RegExpImpl>
    {
        Regex _regex;

        public RegExpImpl(string pattern)
        {
            _regex = new Regex(pattern);
        }

        [ContextMethod("Соответствует", "IsMatch")]
        public IValue IsMatch(string input)
        {
            return ValueFactory.Create(_regex.IsMatch(input));
        }

        [ScriptConstructor(Name = "По регулярному выражению")]
        public static IRuntimeContextInstance Constructor(IValue pattern)
        {
            var regex = new RegExpImpl(pattern.AsString());
            return regex;
        }

    }
}
