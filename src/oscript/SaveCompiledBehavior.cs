/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using OneScript.Language;
using ScriptEngine.Compiler;
using ScriptEngine.Compiler.Packaged;
using ScriptEngine.Machine;

namespace oscript
{
    /// <summary>
    /// Поведение для сохранения скомпилированного модуля/бандла в файл
    /// </summary>
    internal class SaveCompiledBehavior : AppBehavior
    {
        private readonly string _sourcePath;
        private readonly string _outputPath;

        public SaveCompiledBehavior(string sourcePath, string outputPath)
        {
            _sourcePath = sourcePath;
            _outputPath = outputPath;
        }

        public override int Execute()
        {
            if (!File.Exists(_sourcePath))
            {
                Output.WriteLine($"Script file is not found '{_sourcePath}'");
                return 2;
            }

            try
            {
                var builder = ConsoleHostBuilder.Create(_sourcePath);
                var hostedScript = ConsoleHostBuilder.Build(builder);
                hostedScript.Initialize();

                var source = hostedScript.Loader.FromFile(_sourcePath);
                var compiler = hostedScript.GetCompilerService();
                hostedScript.SetGlobalEnvironment(new DoNothingHost(), source);

                var process = hostedScript.Engine.NewProcess();

                // Используем BundleBuilder для сборки всех зависимостей
                var bundleBuilder = new BundleBuilder(hostedScript.Engine, compiler);
                var package = bundleBuilder.Build(source, process);

                var outputPath = string.IsNullOrEmpty(_outputPath)
                    ? Path.ChangeExtension(_sourcePath, ".osc")
                    : _outputPath;

                using (var stream = File.Create(outputPath))
                {
                    bundleBuilder.Save(stream, package);
                }

                var moduleCount = package.Scripts.Count;
                var libModules = moduleCount - 1; // минус entry point
                
                if (libModules > 0)
                {
                    Output.WriteLine($"Bundle saved to: {outputPath} ({libModules} library module(s) included)");
                }
                else
                {
                    Output.WriteLine($"Compiled module saved to: {outputPath}");
                }
                
                return 0;
            }
            catch (ScriptException e)
            {
                Output.WriteLine(e.Message);
                return 1;
            }
            catch (Exception e)
            {
                Output.WriteLine($"Error: {e.Message}");
                return 1;
            }
        }

        public static AppBehavior Create(CmdLineHelper helper)
        {
            var sourcePath = helper.Next();
            if (sourcePath == null)
            {
                Output.WriteLine("Source file path is required");
                return null;
            }

            // Опциональный путь для выходного файла
            string outputPath = null;
            var next = helper.Next();
            if (next != null && !next.StartsWith("-"))
            {
                outputPath = next;
            }

            return new SaveCompiledBehavior(sourcePath, outputPath);
        }
    }
}
