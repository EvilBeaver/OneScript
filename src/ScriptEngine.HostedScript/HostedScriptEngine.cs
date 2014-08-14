using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Environment;
using ScriptEngine.HostedScript;
using ScriptEngine.Machine.Library;

namespace ScriptEngine
{
    public class HostedScriptEngine
    {
        ScriptingEngine _engine;
        GlobalContext _globalCtx;

        public HostedScriptEngine()
        {
            Initialize(new RuntimeEnvironment());
        }

        public HostedScriptEngine(RuntimeEnvironment globalEnvironment)
        {
            Initialize(globalEnvironment);
        }

        private void Initialize(RuntimeEnvironment globalEnvironment)
        {
            _engine = new ScriptingEngine();
            _engine.AttachAssembly(System.Reflection.Assembly.GetExecutingAssembly());

            _globalCtx = new GlobalContext();
            _globalCtx.EngineInstance = _engine;

            globalEnvironment.InjectObject(_globalCtx, false);

            _engine.Initialize(globalEnvironment);
        }

        public void AttachAssembly(System.Reflection.Assembly asm)
        {
            _engine.AttachAssembly(asm);
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
            return _engine.GetCompilerService();
        }

        public Process CreateProcess(IHostApplication host, ICodeSource src)
        {
            var compilerSvc = _engine.GetCompilerService();
            return CreateProcess(host, src, compilerSvc);
        }

        public Process CreateProcess(IHostApplication host, ICodeSource src, CompilerService compilerSvc)
        {
            var module = _engine.LoadModuleImage(compilerSvc.CreateModule(src));
            return InitProcess(host, src, ref module);
        }

        public Process CreateProcess(IHostApplication host, ModuleHandle moduleHandle)
        {
            var module = _engine.LoadModuleImage(moduleHandle);
            return InitProcess(host, null, ref module);
        }

        public Process CreateProcess(IHostApplication host, ModuleHandle moduleHandle, ICodeSource src)
        {
            var module = _engine.LoadModuleImage(moduleHandle);
            return InitProcess(host, src, ref module);
        }

        private Process InitProcess(IHostApplication host, ICodeSource src, ref LoadedModuleHandle module)
        {
            _globalCtx.ApplicationHost = host;
            _globalCtx.CodeSource = src;
            _globalCtx.InitInstance();
            var process = new Process(host, module, _engine);
            return process;
        }

    }
}
