/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace oscript
{
    static class BehaviorSelector
    {
        public static AppBehavior Select(string[] cmdLineArgs)
        {
            var helper = new CmdLineHelper(cmdLineArgs);
            string arg = helper.Next();

            if(arg == null)
                return new ShowUsageBehavior();

            if (!arg.StartsWith("-"))
            {
                var path = arg;
                return new ExecuteScriptBehavior(path, helper.Tail());
            }

            var selected = SelectParametrized(helper);
            if(selected == null)
                selected = new ShowUsageBehavior();

            return selected;
            
        }

        private static AppBehavior SelectParametrized(CmdLineHelper helper)
        {
            var initializers = new Dictionary<string, Func<CmdLineHelper, AppBehavior>>();
            
            initializers.Add("-measure", MeasureBehavior.Create);
            initializers.Add("-compile", ShowCompiledBehavior.Create);
            initializers.Add("-check", CheckSyntaxBehavior.Create);
            initializers.Add("-make", MakeAppBehavior.Create);
            initializers.Add("-cgi", h => new CgiBehavior());
            initializers.Add("-version", h => new ShowVersionBehavior());
            initializers.Add("-v", h => new ShowVersionBehavior());
            initializers.Add("-encoding", ProcessEncodingKey);
            initializers.Add("-codestat", EnableCodeStatistics);
            initializers.Add("-debug", DebugBehavior.Create);
            initializers.Add("-serialize", SerializeModuleBehavior.Create);

            var param = helper.Parse(helper.Current());
            if(initializers.TryGetValue(param.Name.ToLowerInvariant(), out var action))
            {
                return action(helper);
            }

            return null;
        }

        private static AppBehavior EnableCodeStatistics(CmdLineHelper helper)
        {
            var param = helper.Parse(helper.Current());
            if (string.IsNullOrEmpty(param.Value))
                return null;
            
            var outputStatFile = param.Value;
            ScriptFileHelper.EnableCodeStatistics(outputStatFile);
            return Select(helper.Tail());

        }
        
        private static AppBehavior ProcessEncodingKey(CmdLineHelper helper)
        {
            var param = helper.Parse(helper.Current());
            if (!string.IsNullOrEmpty(param.Value))
            {
                var encValue = param.Value;
                Encoding encoding;
                try
                {
                    encoding = Encoding.GetEncoding(encValue);
                }
                catch
                {
                    Output.WriteLine("Wrong console encoding");
                    encoding = null;
                }

                if (encoding != null)
                    Program.ConsoleOutputEncoding = encoding;

                return Select(helper.Tail());
            }

            return null;
        }
    }

}
