/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ScriptEngine.Environment;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine;
using oscript;

namespace StandaloneRunner
{
    class StandaloneProcess : ScriptEngine.HostedScript.IHostApplication
    {
        public int Run()
        {
            try
            {
                ScriptModuleHandle module;
                var engine = new HostedScriptEngine();
                engine.Initialize();

                using(Stream codeStream = LocateCode())
                using (var binReader = new BinaryReader(codeStream))
                {
                    int modulesCount;
                    modulesCount = binReader.ReadInt32();


                    var formatter = new BinaryFormatter();
                    var reader = new ScriptEngine.Compiler.ModulePersistor(formatter);

                    var entry = reader.Read(codeStream);
                    --modulesCount;

                    while (modulesCount-- > 0)
                    {
                        var userScript = reader.Read(codeStream);
                        engine.LoadUserScript(userScript);
                    }

                    module = entry.Module;

                }

                var src = new BinaryCodeSource(module);
                var process = engine.CreateProcess(this, module, src);

                return process.Start();
                
            }
            catch (ScriptInterruptionException e)
            {
                return e.ExitCode;
            }
            catch (Exception e)
            {
                this.ShowExceptionInfo(e);
                return 1;
            }

        }

        public string[] CommandLineArguments { get; set; }

        private Stream LocateCode()
        {
            var fileName = System.Reflection.Assembly.GetExecutingAssembly().Location;
            using(var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                const int SIGN_SIZE = 8;
                fs.Position = fs.Length - SIGN_SIZE;
                byte[] signature = new byte[SIGN_SIZE];
                fs.Read(signature, 0, SIGN_SIZE);

                if (signature[0] == 0x4f && signature[1] == 0x53 && signature[2] == 0x4d && signature[3] == 0x44)
                {
                    int codeOffset = BitConverter.ToInt32(signature, 4);
                    long codeLen = fs.Length - codeOffset - SIGN_SIZE;

                    fs.Seek(codeOffset, SeekOrigin.Begin);
                    byte[] code = new byte[codeLen];
                    fs.Read(code, 0, (int)codeLen);
                    var ms = new MemoryStream(code);

                    return ms;
                }
                else
                {
                    throw new InvalidOperationException("No module found");
                }

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
            else
                return new string[0];
        }

        #endregion

    }

    class BinaryCodeSource : ScriptEngine.Environment.ICodeSource
    {
        private ScriptModuleHandle _mh;

        public BinaryCodeSource(ScriptModuleHandle mh)
        {
            _mh = mh;
        }

        #region ICodeSource Members

        public string SourceDescription
        {
            get { return "Compiled binary module"; }
        }

        public string Code
        {
            get { return "<Source is not available>"; }
        }

        #endregion
    }
}
