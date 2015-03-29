using ScriptEngine.Compiler;
using ScriptEngine.Machine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript
{
    class LibraryLoader : IDirectiveResolver
    {
        private const string USE_DIRECTIVE_RU = "использовать";
        private const string USE_DIRECTIVE_EN = "use";

        private RuntimeEnvironment _env;
        private ScriptingEngine _engine;

        public LibraryLoader(ScriptingEngine engine, RuntimeEnvironment env)
        {
            _env = env;
            _engine = engine;
        }

        public string LibraryRoot { get; set; }

        public bool Resolve(string directive, string value)
        {
            if (DirectiveSupported(directive))
            {
                LoadLibrary(value);
                return true;
            }
            else
                return false;
        }

        private bool DirectiveSupported(string directive)
        {
            return StringComparer.InvariantCultureIgnoreCase.Compare(directive, USE_DIRECTIVE_RU) == 0
                || StringComparer.InvariantCultureIgnoreCase.Compare(directive, USE_DIRECTIVE_EN) == 0;
        }

        private void LoadLibrary(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Ошибка в имени библиотеки", "value");

            bool loaded;
            if (IsQuoted(value))
                loaded = LoadByPath(value.Substring(1, value.Length - 2));
            else
                loaded = LoadByName(value);

            if(!loaded)
                throw new CompilerException(String.Format("Библиотека не найдена {0}", value));

        }

        private bool IsQuoted(string value)
        {
            const char QUOTE = '"';
            if(value[0] == QUOTE && value.Length > 1)
            {
                return value[0] == QUOTE && value[value.Length - 1] == QUOTE;
            }
            else
                return false;
            
        }

        private bool LoadByPath(string libraryPath)
        {
            if (Directory.Exists(libraryPath))
            {
                return LoadLibraryInternal(libraryPath);
            }

            return false;
        }

        private bool LoadByName(string value)
        {
            if (LibraryRoot == null)
                LibraryRoot = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            var libraryPath = Path.Combine(LibraryRoot, value);
            return LoadByPath(libraryPath);
        }

        private bool LoadLibraryInternal(string libraryPath)
        {
            var files = Directory.EnumerateFiles(libraryPath, "*.os")
                .Select(x=>new {Name = Path.GetFileNameWithoutExtension(x), Path = x})
                .Where(x => Utils.IsValidIdentifier(x.Name));
            
            bool hasFiles = false;

            foreach (var file in files)
            {
                hasFiles = true;
                var compiler = _engine.GetCompilerService();
                var instance = (IValue)_engine.AttachedScriptsFactory.LoadFromPath(compiler, file.Path);
                _env.InjectGlobalProperty(instance, file.Name, true);
            }

            return hasFiles;
        }
    }
}
