using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace StandaloneRunner
{
    class StandaloneProcess : ScriptEngine.HostedScript.IHostApplication
    {
        public int Run()
        {
            try
            {
                Stream codeStream = LocateCode();
                var formatter = new BinaryFormatter();
                var reader = new ScriptEngine.Compiler.ModulePersistor(formatter);
                var moduleHandle = reader.Read(codeStream);
                var engine = new ScriptEngine.HostedScriptEngine();
                var src = new BinaryCodeSource(moduleHandle);

                var process = engine.CreateProcess(this, moduleHandle, src);
                return process.Start();
                
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
                return -1;
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

        public void Echo(string text)
        {
            Console.WriteLine(text);
        }

        public void ShowExceptionInfo(Exception exc)
        {
           Console.WriteLine(exc.ToString());
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
            if (CommandLineArguments != null)
                return CommandLineArguments;
            else
                return new string[0];
        }

        #endregion

    }

    class BinaryCodeSource : ScriptEngine.Environment.ICodeSource
    {
        private ScriptEngine.ModuleHandle _mh;

        public BinaryCodeSource(ScriptEngine.ModuleHandle mh)
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
