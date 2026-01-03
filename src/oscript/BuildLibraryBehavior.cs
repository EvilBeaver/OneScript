/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using OneScript.Language;
using ScriptEngine.Compiler.Packaged;
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
                // Создаём скрипт-заглушку который загружает библиотеку
                var loaderScript = $"#Использовать \"{_libraryPath.Replace("\\", "\\\\")}\"";
                
                var builder = ConsoleHostBuilder.Create(_libraryPath);
                var hostedScript = ConsoleHostBuilder.Build(builder);
                hostedScript.Initialize();

                var process = hostedScript.Engine.NewProcess();
                var compiler = hostedScript.GetCompilerService();

                // Компилируем скрипт-заглушку — это загрузит библиотеку в контекст
                var loaderSource = hostedScript.Engine.Loader.FromString(loaderScript);
                compiler.Compile(loaderSource, process);

                // Теперь все модули библиотеки в контексте, можно компилировать
                var libraryBuilder = new LibraryBuilder(hostedScript.Engine, compiler);
                var package = libraryBuilder.Build(_libraryPath, process);

                var outputPath = string.IsNullOrEmpty(_outputPath)
                    ? _libraryPath + ".oslib"
                    : _outputPath;

                using (var stream = File.Create(outputPath))
                {
                    libraryBuilder.Save(stream, package);
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
