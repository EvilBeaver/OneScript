/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
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

namespace StandaloneRunner
{
    internal class StandaloneProcess : IHostApplication
    {
        public string[] CommandLineArguments { get; set; }

        private Stream _sourceStream;

        public int Run()
        {
            if (_sourceStream == null && CommandLineArguments != null && CommandLineArguments.Length > 1)
            {
                var firstArg = CommandLineArguments[0];
                if (firstArg == "-loadDump")
                {
                    var path = CommandLineArguments[1];
                    CommandLineArguments = CommandLineArguments.Skip(2).ToArray();
                    using (var dumpStream = new FileStream(path, FileMode.Open))
                    {
                        _sourceStream = GetCodeStream(dumpStream);
                    }
                    
                    return Run(); //ну да, говнокод и лапша, время жмет
                }
            }

            if (_sourceStream == null)
                _sourceStream = LocateCode();

            var engine = new HostedScriptEngine();
            var src = new BinaryCodeSource();
            engine.SetGlobalEnvironment(this, src);

            try
            {
                
                var templateStorage = new TemplateStorage(new StandaloneTemplateFactory());
                engine.InitializationCallback = (e, env) =>
                {
                    e.Environment.InjectObject(templateStorage);
                    GlobalsManager.RegisterInstance(templateStorage);
                };
                
                engine.Initialize();

                var serializer = new BinaryFormatter();
                var appDump = (ApplicationDump)serializer.Deserialize(_sourceStream);
                foreach (var resource in appDump.Resources)
                {
                    templateStorage.RegisterTemplate(resource.ResourceName, DeserializeTemplate(resource.Data));
                }
                var module = appDump.Scripts[0].Image;

                //module.ModuleInfo = new ModuleInformation { ModuleName = "<исходный код недоступен>", Origin = "<исходный код недоступен>" };

                for (int i = 1; i < appDump.Scripts.Length; i++)
                {
		    //appDump.Scripts[i].Image.ModuleInfo = new ModuleInformation { ModuleName = "<исходный код недоступен1>", Origin = "<исходный код недоступен1>" };
                    engine.LoadUserScript(appDump.Scripts[i]);
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

        private ITemplate DeserializeTemplate(byte[] resourceData)
        {
            byte[] templateBytes;
            TemplateKind kind;
            using (var ms = new MemoryStream(resourceData))
            {
                var br = new BinaryReader(ms);
                var tKind = br.ReadString();
                switch (tKind)
                {
                    case "File":
                        kind = TemplateKind.File;
                        break;
                    case "BinaryData":
                        kind = TemplateKind.BinaryData;
                        break;
                    default:
                        throw new Exception($"Unknown template kind {tKind}");
                }

                var len = br.ReadInt32();
                templateBytes = br.ReadBytes(len);
            }

            return new InternalTemplate(templateBytes, kind);
        }

        private Stream GetCodeStream(Stream sourceStream)
        {
            const int SIGN_SIZE = 8;
            sourceStream.Position = sourceStream.Length - SIGN_SIZE;
            var signature = new byte[SIGN_SIZE];
            sourceStream.Read(signature, 0, SIGN_SIZE);

            if (signature[0] == 0x4f && signature[1] == 0x53 && signature[2] == 0x4d && signature[3] == 0x44)
            {
                var codeOffset = BitConverter.ToInt32(signature, 4);
                var codeLen = sourceStream.Length - codeOffset - SIGN_SIZE;

                sourceStream.Seek(codeOffset, SeekOrigin.Begin);
                var code = new byte[codeLen];
                sourceStream.Read(code, 0, (int)codeLen);
                var ms = new MemoryStream(code);

                return ms;
            }

            throw new InvalidOperationException("No module found");
        }

        private Stream LocateCode()
        {
            var fileName = Assembly.GetExecutingAssembly().Location;
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                return GetCodeStream(fs);
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