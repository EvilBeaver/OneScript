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
            }
            
            return new ShowUsageBehavior();
            
        }
    }

}
