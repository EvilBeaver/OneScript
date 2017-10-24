/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Reflection;

using oscript;

using ScriptEngine.Compiler;
using ScriptEngine.Environment;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;

namespace StandaloneRunner
{
    internal class StandaloneProcess : IHostApplication
    {
        public string[] CommandLineArguments { get; set; }

        public int Run()
        {
            var engine = new HostedScriptEngine();
            var src = new BinaryCodeSource();
            engine.SetGlobalEnvironment(this, src);

            try
            {
                ScriptModuleHandle module;
                engine.Initialize();

                using (var codeStream = LocateCode())
                using (var binReader = new BinaryReader(codeStream))
                {
                    var modulesCount = binReader.ReadInt32();

                    var reader = new ModulePersistor();

                    var entry = reader.Read(codeStream);
                    --modulesCount;

                    while (modulesCount-- > 0)
                    {
                        var userScript = reader.Read(codeStream);
                        engine.LoadUserScript(userScript);
                    }

                    module = entry.Module;
                }

                var process = engine.CreateProcess(this, module, src);

                return process.Start();
            }
            catch (ScriptInterruptionException e)
            {
                return e.ExitCode;
            }
            catch (Exception e)
            {
                ShowExceptionInfo(e);
                return 1;
            }
        }

        private Stream LocateCode()
        {
            var fileName = Assembly.GetExecutingAssembly().Location;
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                const int SIGN_SIZE = 8;
                fs.Position = fs.Length - SIGN_SIZE;
                var signature = new byte[SIGN_SIZE];
                fs.Read(signature, 0, SIGN_SIZE);

                if (signature[0] == 0x4f && signature[1] == 0x53 && signature[2] == 0x4d && signature[3] == 0x44)
                {
                    var codeOffset = BitConverter.ToInt32(signature, 4);
                    var codeLen = fs.Length - codeOffset - SIGN_SIZE;

                    fs.Seek(codeOffset, SeekOrigin.Begin);
                    var code = new byte[codeLen];
                    fs.Read(code, 0, (int)codeLen);
                    var ms = new MemoryStream(code);

                    return ms;
                }

                throw new InvalidOperationException("No module found");
            }
        }

        #region IHostApplication Members

        public void Echo(string text, MessageStatusEnum status = MessageStatusEnum.Ordinary)
        {
            ConsoleHostImpl.Echo(text, status);
        }

        public void ShowExceptionInfo(Exception exc)
        {
            ConsoleHostImpl.ShowExceptionInfo(exc);
        }

        public bool InputString(out string result, int maxLen)
        {
            return ConsoleHostImpl.InputString(out result, maxLen);
        }

        public string[] GetCommandLineArguments()
        {
            if (CommandLineArguments != null)
                return CommandLineArguments;

            return new string[0];
        }

        #endregion
    }

    internal class BinaryCodeSource : ICodeSource
    {
        #region ICodeSource Members

        public string SourceDescription => Assembly.GetExecutingAssembly().Location;

        public string Code => "<Source is not available>";

        #endregion
    }
}