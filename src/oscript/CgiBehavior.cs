using ScriptEngine;
using ScriptEngine.HostedScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oscript
{
    class CgiBehavior : AppBehavior
    {
        public override int Execute()
        {
            string scriptFile;
            //scriptFile = Environment.GetEnvironmentVariable("PATH_TRANSLATED");
            //if(scriptFile == null)
            //{
            //    scriptFile = Environment.GetEnvironmentVariable("SCRIPT_FILENAME");
            //}

            //if(scriptFile == null)
            //{
            //    Console.WriteLine("No CGI Variables found");
            //    return 1;
            //}

            //if(!System.IO.File.Exists(scriptFile))
            //{
            //    Console.WriteLine("Script file not found: {0}", scriptFile);
            //    return 1;
            //}

            scriptFile = @"c:\inetpub\wwwroot\script.os";

            return RunCGIMode(scriptFile);

        }

        private int RunCGIMode(string scriptFile)
        {
            var env = new RuntimeEnvironment();
            var engine = new HostedScriptEngine(env);
            engine.AttachAssembly(System.Reflection.Assembly.GetExecutingAssembly());

            var request = new WebRequestContext();
            env.InjectGlobalProperty(request, "ВебЗапрос", true);
            
            var source = engine.Loader.FromFile(scriptFile);
            var host = new CGIHost();
            Process process;

            try
            {
                process = engine.CreateProcess(host, source);
            }
            catch (Exception e)
            {
                Console.WriteLine("Content-type: text/plain");
                Console.WriteLine("Content-encoding: utf-8");
                Console.WriteLine();

                host.ShowExceptionInfo(e);
                return 1;
            }

            return process.Start();

        }
    }
}
