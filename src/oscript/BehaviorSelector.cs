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
                return ProcessCheckKey(helper);
            }
            else if (param == "-make")
            {
                var codepath = helper.Next();
                var output = helper.Next();
                var makeBin = helper.Next();
                if (output != null && codepath != null)
                {
                    var appMaker = new MakeAppBehavior(codepath, output);
                    if (makeBin != null && makeBin == "-bin")
                        appMaker.CreateDumpOnly = true;

                    return appMaker;
                }
            }
            else if (param == "-cgi")
            {
                return new CgiBehavior();
            }
            else if (param == "-version" || param == "-v")
            {
                return new ShowVersionBehavior();
            }
            else if (param.StartsWith("-encoding="))
            {
                return ProcessEncodingKey(helper);
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
            else if (param == "-debug")
            {
                var arg = helper.Next();
                int port = 2801;
                if (arg != null && arg.StartsWith("-port="))
                {
                    var prefixLen = ("-port=").Length;
                    if (arg.Length > prefixLen)
                    {
                        var value = arg.Substring(prefixLen);
                        if (!Int32.TryParse(value, out port))
                        {
                            Output.WriteLine("Incorrect port: " + value);
                            return null;
                        }
                    }
                }
                else if(arg != null)
                {
                    var path = arg;
                    return new DebugBehavior(port, path, helper.Tail());
                }
            }
            else if (param == "-serialize")
            {
                var path = helper.Next();
                if (path != null)
                {
                    return new SerializeModuleBehavior(path);
                }

                return new ShowUsageBehavior();
            }

            return null;
        }

        private static AppBehavior ProcessCheckKey(CmdLineHelper helper)
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

            return null;
        }

        private static AppBehavior ProcessEncodingKey(CmdLineHelper helper)
        {
            var param = helper.Current();
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

            return null;
        }
    }

}
