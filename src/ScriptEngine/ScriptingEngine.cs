using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Compiler;
using ScriptEngine.Environment;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine.Library;

namespace ScriptEngine
{
    public class ScriptingEngine : IDisposable
    {
        private MachineInstance _machine = new MachineInstance();
        private ScriptSourceFactory _scriptFactory;
        private AttachedScriptsFactory _attachedScriptsFactory;
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

            _attachedScriptsFactory = new AttachedScriptsFactory(this);
            AttachedScriptsFactory.SetInstance(_attachedScriptsFactory);
        }

        public ICodeSourceFactory Loader
        {
            get
            {
                return _scriptFactory;
            }
        }

        public CompilerService GetCompilerService()
        {
            return new CompilerService(_symbolsContext);
        }

        public LoadedModuleHandle LoadModuleImage(ModuleHandle moduleImage)
        {
            var handle = new LoadedModuleHandle();
            handle.Module = new LoadedModule(moduleImage.Module);
            return handle;
        }

        internal IRuntimeContextInstance NewObject(LoadedModule module)
        {
            var scriptContext = new Machine.Contexts.UserScriptContextInstance(module);
            scriptContext.Initialize(_machine);

            return scriptContext;
        }

        public IRuntimeContextInstance NewObject(LoadedModuleHandle module)
        {
            return NewObject(module.Module); 
        }

        public void InitializeSDO(ScriptDrivenObject sdo)
        {
            sdo.Initialize(_machine);
        }

        public void ExecuteModule(LoadedModuleHandle module)
        {
            NewObject(module);
        }

        public MachineInstance Machine
        {
            get { return _machine; }
        }

        public AttachedScriptsFactory AttachedScriptsFactory
        {
            get
            {
                return _attachedScriptsFactory;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            AttachedScriptsFactory.Dispose();
        }

        #endregion
    }
}
