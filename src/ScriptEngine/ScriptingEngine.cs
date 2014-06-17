using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Compiler;
using ScriptEngine.Environment;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Library;

namespace ScriptEngine
{
    public class ScriptingEngine
    {
        private MachineInstance _machine = new MachineInstance();
        private ScriptSourceFactory _scriptFactory;
        private CompilerContext _symbolsContext;

        public ScriptingEngine()
        {
            TypeManager.Initialize(new StandartTypeManager());
            ContextDiscoverer.Discover(System.Reflection.Assembly.GetExecutingAssembly());
        }

        public void AttachAssembly(System.Reflection.Assembly asm)
        {
            ContextDiscoverer.Discover(asm);
        }

        public void Initialize(RuntimeEnvironment environment)
        {
            _scriptFactory = new ScriptSourceFactory();
            _symbolsContext = environment.SymbolsContext;

            foreach (var item in environment.AttachedContexts)
            {
                _machine.AttachContext(item, false);
            }
        }

        public ICodeSourceFactory Loader
        {
            get
            {
                return _scriptFactory;
            }
        }

        public CompilerService GetCompilationService()
        {
            return new CompilerService(_symbolsContext);
        }

        public LoadedModuleHandle LoadModuleImage(ModuleHandle moduleImage)
        {
            var handle = new LoadedModuleHandle();
            handle.Module = new LoadedModule(moduleImage.Module);
            return handle;
        }

        public IRuntimeContextInstance NewObject(LoadedModuleHandle module)
        {
            var scriptContext = new Machine.Contexts.UserScriptContextInstance(module.Module);
            _machine.StateConsistentOperation(() =>
                {
                    _machine.SetModule(module.Module);
                    _machine.AttachContext(scriptContext, true);
                    _machine.ExecuteModuleBody();
                });

            return scriptContext;
            
        }

        public void ExecuteModule(LoadedModuleHandle module)
        {
            NewObject(module);
        }

        public MachineInstance Machine
        {
            get { return _machine; }
        }

    }
}
