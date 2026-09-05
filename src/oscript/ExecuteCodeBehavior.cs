/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using ScriptEngine.HostedScript;

namespace oscript
{
    internal class ExecuteCodeBehavior(string code, string[] args) : AppBehavior
    {
        private readonly ConsoleApplicationHost _host = new(args);

        public static AppBehavior Create(CmdLineHelper helper)
        {
            var code = helper.Next();
            if (string.IsNullOrEmpty(code))
                return null;

            return new ExecuteCodeBehavior(code, helper.Tail());
        }

        public override int Execute()
        {
            var configPath = Path.Combine(Environment.CurrentDirectory, CfgFileConfigProvider.CONFIG_FILE_NAME);
            var builder = ConsoleHostBuilder.Create(configPath);
            using var hostedScript = ConsoleHostBuilder.Build(builder);
            var source = hostedScript.Loader.FromString(code);

            return _host.RunProcess(hostedScript, source);
        }
    }
}
