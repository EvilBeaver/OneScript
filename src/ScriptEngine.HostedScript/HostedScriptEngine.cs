using System;
using ScriptEngine.Environment;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript
{
    public class HostedScriptEngine
    {
        ScriptingEngine _engine;
        SystemGlobalContext _globalCtx;
        RuntimeEnvironment _env;
        bool _isInitialized;

        public HostedScriptEngine()
        {
            _engine = new ScriptingEngine();
            _env = new RuntimeEnvironment();
            _engine.AttachAssembly(System.Reflection.Assembly.GetExecutingAssembly(), _env);

            _globalCtx = new SystemGlobalContext();
            _globalCtx.EngineInstance = _engine;

            _env.InjectObject(_globalCtx, false);
            var libLoader = new LibraryResolver(_engine, _env);
            libLoader.LibraryRoot = LibraryRoot;
            _engine.DirectiveResolver = libLoader;
            _engine.Environment = _env;
        }

        public void Initialize()
        {
            if (!_isInitialized)
            {
                _engine.Initialize();
                TypeManager.RegisterType("Сценарий", typeof(UserScriptContextInstance));

                _isInitialized = true;
            }
        }

        public void AttachAssembly(System.Reflection.Assembly asm)
        {
            _engine.AttachAssembly(asm, _env);
        }

        public void InjectGlobalProperty(string name, IValue value, bool readOnly)
        {
            _env.InjectGlobalProperty(value, name, readOnly);
        }

        public void InjectObject(IAttachableContext obj, bool asDynamicScope)
        {
            _env.InjectObject(obj, asDynamicScope);
        }

        public ICodeSourceFactory Loader
        {
            get
            {
                return _engine.Loader;
            }
        }

        public CompilerService GetCompilerService()
        {
            var compilerSvc = _engine.GetCompilerService();
            compilerSvc.DefineVariable("ЭтотОбъект", SymbolType.ContextProperty);
            return compilerSvc;
        }

        public string LibraryRoot { get; set; }

        public Process CreateProcess(IHostApplication host, ICodeSource src)
        {
            return CreateProcess(host, src, GetCompilerService());
        }

        public Process CreateProcess(IHostApplication host, ICodeSource src, CompilerService compilerSvc)
        {
            SetGlobalEnvironment(host, src);
            var module = _engine.LoadModuleImage(compilerSvc.CreateModule(src));
            return InitProcess(host, src, ref module);
        }

        public Process CreateProcess(IHostApplication host, ScriptModuleHandle moduleHandle, ICodeSource src)
        {
            SetGlobalEnvironment(host, src);
            var module = _engine.LoadModuleImage(moduleHandle);
            return InitProcess(host, src, ref module);
        }

        private void SetGlobalEnvironment(IHostApplication host, ICodeSource src)
        {
            _globalCtx.ApplicationHost = host;
            _globalCtx.CodeSource = src;
            _globalCtx.InitInstance();
        }

        private Process InitProcess(IHostApplication host, ICodeSource src, ref LoadedModuleHandle module)
        {
            Initialize();
            var process = new Process(host, module, _engine);
            return process;
        }

    }
}
