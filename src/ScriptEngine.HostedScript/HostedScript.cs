using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Environment;
using ScriptEngine.Machine.Library;

namespace ScriptEngine.HostedScript
{
    public class HostedScript
    {
        ScriptingEngine _engine;
        
        public void Initialize()
        {
            var env = new RuntimeEnvironment();
            var globalCtx = new GlobalContext();
            env.InjectObject(globalCtx, globalCtx);
            _engine = new ScriptingEngine();
            _engine.Initialize(env);
        }

        ICodeSourceFactory Loader
        {
            get
            {
                return _engine.Loader;
            }
        }

        public Process CreateProcess(IHostApplication host, LoadedModuleHandle src)
        {

        }
    }
}
