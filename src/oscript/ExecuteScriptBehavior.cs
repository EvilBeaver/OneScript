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
using ScriptEngine;
using ScriptEngine.Compiler;
using ScriptEngine.HostedScript;
using ScriptEngine.Machine;

namespace oscript
{
    class ExecuteScriptBehavior : AppBehavior, IHostApplication
    {
        string[] _scriptArgs;
        string _path;

        public ExecuteScriptBehavior(string path, string[] args)
        {
            _scriptArgs = args;
            _path = path;
        }
        
        public override int Execute()
        {
            if (!System.IO.File.Exists(_path))
            {
                throw new System.IO.FileNotFoundException("Script file is not found", _path);
            }

            var hostedScript = new HostedScriptEngine();
            hostedScript.Initialize();
            var source = hostedScript.Loader.FromFile(_path);

            Process process;
            try
            {
                process = hostedScript.CreateProcess(this, source);
            }
            catch(Exception e)
            {
                this.ShowExceptionInfo(e);
                return 1;
            }

            return process.Start();
        }

        #region IHostApplication Members

        public void Echo(string text)
        {
            if(Program.ConsoleOutputEncoding == null)
                Console.WriteLine(text);
            else
            {
                using(var stdout = Console.OpenStandardOutput())
                {
                    var enc = Program.ConsoleOutputEncoding;
                    var bytes = enc.GetBytes(text);
                    stdout.Write(bytes, 0, bytes.Length);
                    stdout.WriteByte(13);
                    stdout.WriteByte(10);
                }
            }
        }

        public void ShowExceptionInfo(Exception exc)
        {
            if(exc is ScriptException)
            {
                var rte = (ScriptException)exc;
                Console.WriteLine(rte.MessageWithoutCodeFragment);
            }
            else
                Console.WriteLine(exc.Message);
        }

        public bool InputString(out string result, int maxLen)
        {
            if (maxLen == 0)
            {
                result = Console.ReadLine();
            }
            else
            {
                result = Console.ReadLine().Substring(0, maxLen);
            }

            return result.Length > 0;

        }

        public string[] GetCommandLineArguments()
        {
            return _scriptArgs;
        }

        #endregion
    }
}
