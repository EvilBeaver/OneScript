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
            Initialize();
        }

        private void Initialize()
        {
            _engine = new ScriptingEngine();
            _engine.AttachAssembly(System.Reflection.Assembly.GetExecutingAssembly());
            
            var env = new RuntimeEnvironment();
            _globalCtx = new GlobalContext();
            env.InjectObject(_globalCtx);
            
            _engine.Initialize(env);
        }

        public ICodeSourceFactory Loader
        {
            get
            {
                return _engine.Loader;
            }
        }

        public Process CreateProcess(IHostApplication host, ICodeSource src)
        {
            var module = _engine.LoadModule(src.CreateModule());
            _globalCtx.ApplicationHost = host;
            _globalCtx.CodeSource = src;
            _globalCtx.InitInstance();
            var process = new Process(host, module, _engine);
            return process;
        }
    }
}
