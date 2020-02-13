/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

using oscript;
using ScriptEngine;
using ScriptEngine.Compiler;
using ScriptEngine.Environment;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace StandaloneRunner
{
    internal class StandaloneProcess
    {
        public string[] CommandLineArguments { get; set; }

        public int LoadAndRun(Stream codeStream)
        {
            var loader = new ProcessLoader();
            var host = new StandaloneApplicationHost();
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

    internal class BinaryCodeSource : ICodeSource
    {
        #region ICodeSource Members

        public string SourceDescription => Assembly.GetExecutingAssembly().Location;

        public string Code => "<Source is not available>";

        #endregion
    }
}