/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Reflection;
using ScriptEngine.Environment;
using ScriptEngine.Machine;

namespace StandaloneRunner
{
    internal class StandaloneProcess
    {
        public string[] CommandLineArguments { get; set; }

        public int LoadAndRun(Stream codeStream)
        {
            var loader = new ProcessLoader();
            var host = new StandaloneApplicationHost
            {
                CommandLineArguments = CommandLineArguments
            };
            var process = loader.CreateProcess(codeStream, host);
            try
            {
                return process.Start();
            }
            catch (ScriptInterruptionException e)
            {
                return e.ExitCode;
            }
            catch (Exception e)
            {
                host.ShowExceptionInfo(e);
                return 1;
            }
        }
    }

    internal class BinaryCodeSource : OneScript.Sources.ICodeSource
    {
        public string Location => Assembly.GetExecutingAssembly().Location;
        
        public string GetSourceCode() => "<Source is not available>";
    }
}