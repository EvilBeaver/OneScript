/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using OneScript.Execution;
using OneScript.Language;
using ScriptEngine.Compiler.Packaged;
using ScriptEngine.HostedScript;
using ScriptEngine.Libraries;
using ScriptEngine.Machine;

namespace oscript
{
    /// <summary>
    /// Поведение для компиляции библиотеки в .oslib файл
    /// </summary>
    internal class BuildLibraryBehavior : AppBehavior
    {
        private readonly string _libraryPath;
        private readonly string _outputPath;

        public BuildLibraryBehavior(string libraryPath, string outputPath)
        {
            _libraryPath = libraryPath;
            _outputPath = outputPath;
        }

        public override int Execute()
        {
            if (!Directory.Exists(_libraryPath))
            {
                Output.WriteLine($"Library directory not found: '{_libraryPath}'");
                return 2;
            }

            try
            {
                var builder = ConsoleHostBuilder.Create(_libraryPath);
                var hostedScript = ConsoleHostBuilder.Build(builder);
                hostedScript.Initialize();

                var process = hostedScript.Engine.NewProcess();

                // Загружаем библиотеку через LibraryLoader и получаем ExternalLibraryInfo
                var libraryLoader = ScriptEngine.HostedScript.LibraryLoader.Create(hostedScript.Engine, process);
                var libraryInfo = libraryLoader.LoadLibraryWithInfo(_libraryPath, process);

                if (libraryInfo == null)
                {
                    Output.WriteLine($"Failed to load library: '{_libraryPath}'");
                    return 1;
                }

                // Собираем пакет из загруженных модулей
                var packageBuilder = new LibraryBuilder(hostedScript.Engine, hostedScript.GetCompilerService());
                var package = packageBuilder.BuildFromLoaded(libraryInfo);

                var outputPath = string.IsNullOrEmpty(_outputPath)
                    ? _libraryPath + ".oslib"
                    : _outputPath;

                using (var stream = File.Create(outputPath))
                {
                    packageBuilder.Save(stream, package);
                }

                var moduleCount = package.Scripts.Count;
                Output.WriteLine($"Library compiled to: {outputPath} ({moduleCount} module(s))");

                if (package.Dependencies.Count > 0)
                {
                    Output.WriteLine($"Dependencies: {string.Join(", ", package.Dependencies)}");
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
            var libraryPath = helper.Next();
            if (libraryPath == null)
            {
                Output.WriteLine("Library path is required");
                return null;
            }

            // Убираем trailing slash если есть
            libraryPath = libraryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // Опциональный путь для выходного файла
            string outputPath = null;
            var next = helper.Next();
            if (next != null && !next.StartsWith("-"))
            {
                outputPath = next;
            }

            return new BuildLibraryBehavior(libraryPath, outputPath);
        }
    }
}
