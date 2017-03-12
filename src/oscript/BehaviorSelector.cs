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
            var param = helper.Current().ToLowerInvariant();
            if (param == "-measure")
            {
                var path = helper.Next();
                if (path != null)
                {
                    return new MeasureBehavior(path, helper.Tail());
                }
            }
            else if (param == "-compile")
            {
                var path = helper.Next();
                if (path != null)
                {
                    return new ShowCompiledBehavior(path);
                }
            }
            else if (param == "-check")
            {
                if (helper.Next() != null)
                {
                    bool cgi_mode = false;
                    var arg = helper.Current();
                    if (arg.ToLowerInvariant() == "-cgi")
                    {
                        cgi_mode = true;
                        arg = helper.Next();
                    }

                    var path = arg;
                    var env = helper.Next();
                    if (env != null && env.StartsWith("-env="))
                    {
                        env = env.Substring(5);
                    }

                    return new CheckSyntaxBehavior(path, env, cgi_mode);
                }
            }
            else if (param == "-make")
            {
                var codepath = helper.Next();
                var output = helper.Next();

                if (output != null && codepath != null)
                {
                    return new MakeAppBehavior(codepath, output);
                }
            }
            else if (param == "-cgi")
            {
                return new CgiBehavior();
            }
            else if (param == "-version")
            {
                return new ShowVersionBehavior();
            }
            else if (param.StartsWith("-encoding="))
            {
                var prefixLen = ("-encoding=").Length;
                if (param.Length > prefixLen)
                {
                    var encValue = param.Substring(prefixLen);
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
            }
            else if (param.StartsWith("-codestat="))
            {
                var prefixLen = ("-codestat=").Length;
                if (param.Length > prefixLen)
                {
                    var outputStatFile = param.Substring(prefixLen);
                    ScriptFileHelper.EnableCodeStatistics(outputStatFile);
                    return Select(helper.Tail());
                }
            }

            return null;
        }
    }

}
