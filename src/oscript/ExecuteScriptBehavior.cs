/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.IO;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Debugger;

namespace oscript
{
    class ExecuteScriptBehavior : AppBehavior
    {
        protected string _path;
        private readonly ConsoleApplicationHost _host;

        public ExecuteScriptBehavior(string path, string[] args)
        {
            _path = path;
            _host = new ConsoleApplicationHost(args);
        }
        
        public IDebugger DebugController { get; set; } = new DisabledDebugger();
        
        public string CodeStatFile { get; set; }

        public bool CodeStatisticsEnabled { get; set; }

        public override int Execute()
        {
            if (!File.Exists(_path))
            {
                _host.Echo($"Script file is not found '{_path}'");
                return 2;
            }

            var builder = ConsoleHostBuilder.Create(_path);
            builder.WithDebugger(DebugController);
            CodeStatHub codeStatHub = null;
            CodeStatProcessor cliSession = null;
            if (CodeStatisticsEnabled)
            {
                codeStatHub = new CodeStatHub();
                builder.Services.RegisterSingleton(codeStatHub);
                builder.Services.RegisterSingleton<ICodeStatCollector>(codeStatHub);
                if (CodeStatFile != null)
                    cliSession = codeStatHub.StartSession();
            }

            using var hostedScript = ConsoleHostBuilder.Build(builder);
            var source = hostedScript.Loader.FromFile(_path);
            var result = _host.RunProcess(hostedScript, source);

            if (cliSession != null && codeStatHub != null)
            {
                codeStatHub.FinishSession(cliSession);
                var statsWriter = new CodeStatWriter(CodeStatFile, CodeStatWriterType.JSON);
                statsWriter.Write(cliSession.GetStatData());
            }

            return result;
        }
    }
}
