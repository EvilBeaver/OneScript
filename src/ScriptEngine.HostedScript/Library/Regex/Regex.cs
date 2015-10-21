/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using RegExp = System.Text.RegularExpressions;

namespace ScriptEngine.HostedScript.Library.RegexLib
{
    [ContextClass("РегулярноеВыражение", "Regex")]
    class RegExpImpl : AutoContext<RegExpImpl>
    {
        private RegExp.Regex _regex;
        private string _pattern;

        public RegExpImpl(string pattern)
        {
            _pattern = pattern;
            _regex = new RegExp.Regex(_pattern, RegExp.RegexOptions.IgnoreCase | RegExp.RegexOptions.Multiline );
        }

        [ContextMethod("Совпадает", "IsMatch")]
        public IValue IsMatch(string input)
        {
            return ValueFactory.Create(_regex.IsMatch(input));
        }

        [ContextMethod("НайтиСовпадения", "Matches")]
        public IValue Matches(string input)
        {
            return new MatchCollection(_regex.Matches(input));
        }

        [ContextProperty("ИгнорироватьРегистр", "IgnoreCase")]
        public bool IgnoreCase
        {
            get { return _regex.Options.HasFlag( RegExp.RegexOptions.IgnoreCase ); }
            set { SetOption(value, RegExp.RegexOptions.IgnoreCase); }
        }

        [ContextProperty("Многострочный", "Multiline")]
        public bool Multiline
        {
            get { return _regex.Options.HasFlag(RegExp.RegexOptions.Multiline); }
            set { SetOption(value, RegExp.RegexOptions.Multiline); }
        }

        [ScriptConstructor(Name = "По регулярному выражению")]
        public static IRuntimeContextInstance Constructor(IValue pattern)
        {
            var regex = new RegExpImpl(pattern.AsString());
            return regex;
        }

        private void SetOption(bool value, RegExp.RegexOptions option)
        {
                var options = _regex.Options;
                if (value)
                    options |= option;
                else
                    options &= ~option;

                //приходится пересоздавать объект, т.к. опции объекта по умолчанию только read-only
                _regex = new RegExp.Regex(_pattern, options);
        }
    }
}
