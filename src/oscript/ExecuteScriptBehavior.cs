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

        public bool CodeStatisticsEnabled => CodeStatFile != null;

        public override int Execute()
        {
            if (!File.Exists(_path))
            {
                _host.Echo($"Script file is not found '{_path}'");
                return 2;
            }

            var builder = ConsoleHostBuilder.Create(_path);
            builder.WithDebugger(DebugController);
            CodeStatProcessor codeStatProcessor = null;
            if (CodeStatisticsEnabled)
            {
                codeStatProcessor = new CodeStatProcessor();
                builder.Services.RegisterSingleton<ICodeStatCollector>(codeStatProcessor);
            }

            using var hostedScript = ConsoleHostBuilder.Build(builder);
            var source = hostedScript.Loader.FromFile(_path);
            var result = _host.RunProcess(hostedScript, source);

            if (codeStatProcessor != null)
            {
                codeStatProcessor.EndCodeStat();
                var codeStat = codeStatProcessor.GetStatData();
                var statsWriter = new CodeStatWriter(CodeStatFile, CodeStatWriterType.JSON);
                statsWriter.Write(codeStat);
            }

            return result;
        }
    }
}
