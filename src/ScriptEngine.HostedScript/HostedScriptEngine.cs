using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Environment;
using ScriptEngine.HostedScript;
using ScriptEngine.Machine.Library;
using ScriptEngine.Machine;
using ScriptEngine.HostedScript.Library;

namespace ScriptEngine
{
    public class HostedScriptEngine
    {
        ScriptingEngine _engine;
        GlobalContext _globalCtx;
        RuntimeEnvironment _env;
        bool _isInitialized;

        public HostedScriptEngine()
        {
            _engine = new ScriptingEngine();
            _engine.AttachAssembly(System.Reflection.Assembly.GetExecutingAssembly());

            _globalCtx = new GlobalContext();
            _globalCtx.EngineInstance = _engine;

            _env = new RuntimeEnvironment();
            _env.InjectObject(_globalCtx, false);
            _env.InjectGlobalProperty(XmlNodeTypeEnum.GetInstance(), "ТипУзлаXML", true);
        }

        public void Initialize()
        {
            if (!_isInitialized)
            {
                _engine.Initialize(_env);
                _isInitialized = true;
            }
        }

        public void AttachAssembly(System.Reflection.Assembly asm)
        {
            _engine.AttachAssembly(asm);
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
            Initialize();
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
            Initialize();
            _globalCtx.ApplicationHost = host;
            _globalCtx.CodeSource = src;
            _globalCtx.InitInstance();
            var process = new Process(host, module, _engine);
            return process;
        }

    }
}
