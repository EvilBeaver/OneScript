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
    static class BehaviorSelector
    {
        public static AppBehavior Select(string[] cmdLineArgs)
        {
            if (cmdLineArgs.Length == 0)
            {
                return new ShowUsageBehavior();
            }
            else
            {
                if (!cmdLineArgs[0].StartsWith("-"))
                {
                    var path = cmdLineArgs[0];
                    return new ExecuteScriptBehavior(path, cmdLineArgs.Skip(1).ToArray());
                }
                else if (cmdLineArgs[0].ToLower() == "-measure")
                {
                    if (cmdLineArgs.Length > 1)
                    {
                        var path = cmdLineArgs[1];
                        return new MeasureBehavior(path, cmdLineArgs.Skip(2).ToArray());
                    }
                }
                else if (cmdLineArgs[0].ToLower() == "-compile")
                {
                    if (cmdLineArgs.Length > 1)
                    {
                        var path = cmdLineArgs[1];
                        return new ShowCompiledBehavior(path);
                    }
                }
                else if (cmdLineArgs[0].ToLower() == "-make")
                {
                    if (cmdLineArgs.Length == 3)
                    {
                        var codepath = cmdLineArgs[1];
                        var output = cmdLineArgs[2];
                        return new MakeAppBehavior(codepath, output);
                    }
                }
                else if(cmdLineArgs[0].ToLower() == "-cgi")
                {
                    return new CgiBehavior();
                }
            }
            
            return new ShowUsageBehavior();
            
        }
    }

}
