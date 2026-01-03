/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using OneScript.Language.Sources;
using OneScript.Sources;
using OneScript.StandardLibrary;
using ScriptEngine;
using ScriptEngine.Compiler.Packaged;
using ScriptEngine.HostedScript;
using ScriptEngine.Machine;

namespace oscript
{
    /// <summary>
    /// Поведение для выполнения скомпилированного модуля (.osc файла)
    /// </summary>
    internal class ExecuteCompiledBehavior : AppBehavior, IHostApplication, ISystemLogWriter
    {
        private readonly string _path;
        private readonly string[] _scriptArgs;

        public ExecuteCompiledBehavior(string path, string[] args)
        {
            _path = path;
            _scriptArgs = args;
        }

        public override int Execute()
        {
            if (!File.Exists(_path))
            {
                Echo($"Compiled module file is not found '{_path}'");
                return 2;
            }

            SystemLogger.SetWriter(this);

            try
            {
                var builder = ConsoleHostBuilder.Create(_path);
                var hostedScript = ConsoleHostBuilder.Build(builder);
                hostedScript.Initialize();

                // Загружаем скомпилированный модуль
                var packager = new CompiledModulePackager();
                StackRuntimeModule module;

                using (var stream = File.OpenRead(_path))
                {
                    module = packager.Load(stream, hostedScript.Engine.Environment);
                }

                // Создаём source для контекста
                var source = SourceCodeBuilder.Create()
                    .FromSource(new CompiledCodeSource(_path))
                    .WithName(Path.GetFileName(_path))
                    .Build();

                hostedScript.SetGlobalEnvironment(this, source);

                // Создаём и запускаем процесс
                var bslProcess = hostedScript.Engine.NewProcess();
                var scriptObject = hostedScript.Engine.NewObject(module, bslProcess);

                hostedScript.Dispose();
                return 0;
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

        #region IHostApplication Members

        public void Echo(string text, MessageStatusEnum status = MessageStatusEnum.Ordinary)
        {
            ConsoleHostImpl.Echo(text, status);
        }

        public void ShowExceptionInfo(Exception exc)
        {
            ConsoleHostImpl.ShowExceptionInfo(exc);
        }

        public bool InputString(out string result, string prompt, int maxLen, bool multiline)
        {
            return ConsoleHostImpl.InputString(out result, prompt, maxLen, multiline);
        }

        public string[] GetCommandLineArguments()
        {
            return _scriptArgs;
        }

        #endregion

        public void Write(string text)
        {
            Console.Error.WriteLine(text);
        }
    }

    /// <summary>
    /// Источник кода для скомпилированного модуля
    /// </summary>
    internal class CompiledCodeSource : ICodeSource
    {
        public CompiledCodeSource(string location)
        {
            Location = location;
        }

        public string Location { get; }

        public string GetSourceCode() => "<Source code is not available for compiled modules>";
    }
}
