/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oscript
{
    internal static class BehaviorSelector
    {
        public static AppBehavior Select(string[] cmdLineArgs)
        {
            while (true)
            {
                if (cmdLineArgs.Length == 0)
                    return new ShowUsageBehavior();

                if (!cmdLineArgs[0].StartsWith("-"))
                {
                    var path = cmdLineArgs[0];
                    return new ExecuteScriptBehavior(path, cmdLineArgs.Skip(1).ToArray());
                }

                switch (cmdLineArgs[0].ToLower())
                {
                    case "-measure":
                        if (cmdLineArgs.Length > 1)
                        {
                            var path = cmdLineArgs[1];
                            return new MeasureBehavior(path, cmdLineArgs.Skip(2).ToArray());
                        }

                        break;
                    case "-compile":
                        if (cmdLineArgs.Length > 1)
                        {
                            var path = cmdLineArgs[1];
                            return new ShowCompiledBehavior(path);
                        }

                        break;
                    case "-check":
                        if (cmdLineArgs.Length > 1)
                        {
                            var cgiMode = false;
                            var paramIndex = 1;
                            if (cmdLineArgs[paramIndex].ToLower() == "-cgi")
                            {
                                ++paramIndex;
                                cgiMode = true;
                            }

                            var path = cmdLineArgs[paramIndex];
                            ++paramIndex;
                            string env = null;
                            if (cmdLineArgs.Length > paramIndex && cmdLineArgs[paramIndex].StartsWith("-env="))
                                env = cmdLineArgs[paramIndex].Substring(5);

                            return new CheckSyntaxBehavior(path, env, cgiMode);
                        }

                        break;
                    case "-make":
                        if (cmdLineArgs.Length == 3)
                        {
                            var codepath = cmdLineArgs[1];
                            var output = cmdLineArgs[2];
                            return new MakeAppBehavior(codepath, output);
                        }

                        break;
                    case "-cgi":
                        return new CgiBehavior();
                    case "-version":
                        return new ShowVersionBehavior();
                    default:
                        if (cmdLineArgs[0].StartsWith("-encoding="))
                        {
                            var prefixLen = "-encoding=".Length;
                            if (cmdLineArgs[0].Length > prefixLen)
                            {
                                var encValue = cmdLineArgs[0].Substring(prefixLen);
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

                                cmdLineArgs = cmdLineArgs.Skip(1).ToArray();
                                continue;
                            }
                        }
                        else if (cmdLineArgs[0].StartsWith("-codestat="))
                        {
                            var prefixLen = "-codestat=".Length;
                            if (cmdLineArgs[0].Length > prefixLen)
                            {
                                var outputStatFile = cmdLineArgs[0].Substring(prefixLen);
                                ScriptFileHelper.EnableCodeStatistics(outputStatFile);
                                cmdLineArgs = cmdLineArgs.Skip(1).ToArray();
                                continue;
                            }
                        }

                        break;
                }

                return new ShowUsageBehavior();
            }
        }
    }
}