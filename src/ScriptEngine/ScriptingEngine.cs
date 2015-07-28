/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using ScriptEngine.Compiler;
using ScriptEngine.Environment;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine
{
    public class ScriptingEngine : IDisposable
    {
        private MachineInstance _machine = new MachineInstance();
        private ScriptSourceFactory _scriptFactory;
        private AttachedScriptsFactory _attachedScriptsFactory;

        public ScriptingEngine()
        {
            TypeManager.Initialize(new StandartTypeManager());
            GlobalsManager.Reset();
            ContextDiscoverer.DiscoverClasses(System.Reflection.Assembly.GetExecutingAssembly());
            
            _scriptFactory = new ScriptSourceFactory();
        }

        public void AttachAssembly(System.Reflection.Assembly asm)
        {
            ContextDiscoverer.DiscoverClasses(asm);
        }

        public void AttachAssembly(System.Reflection.Assembly asm, RuntimeEnvironment globalEnvironment)
        {
            ContextDiscoverer.DiscoverClasses(asm);
            ContextDiscoverer.DiscoverGlobalContexts(globalEnvironment, asm);
        }

        public RuntimeEnvironment Environment { get; set; }

        public ScriptInformationContext CurrentScript 
        { 
            get
            {
                return _machine.CurrentScript;
            }
        }

        public void Initialize()
        {
            SetDefaultEnvironmentIfNeeded();

            var symbolsContext = Environment.SymbolsContext;
            _machine.Cleanup();
            foreach (var item in Environment.AttachedContexts)
            {
                _machine.AttachContext(item, false);
            }

            _attachedScriptsFactory = new AttachedScriptsFactory(this);
            AttachedScriptsFactory.SetInstance(_attachedScriptsFactory);
        }

        private void SetDefaultEnvironmentIfNeeded()
        {
            if (Environment == null)
                Environment = new RuntimeEnvironment();
        }

        public ICodeSourceFactory Loader
        {
            get
            {
                return _scriptFactory;
            }
        }

        public IDirectiveResolver DirectiveResolver { get; set; }

        public CompilerService GetCompilerService()
        {
            var cs = new CompilerService(Environment.SymbolsContext);
            cs.DirectiveResolver = DirectiveResolver;
            return cs;
        }

        public LoadedModuleHandle LoadModuleImage(ScriptModuleHandle moduleImage)
        {
            var handle = new LoadedModuleHandle();
            handle.Module = new LoadedModule(moduleImage.Module);
            return handle;
        }

        internal IRuntimeContextInstance NewObject(LoadedModule module)
        {
            var scriptContext = new Machine.Contexts.UserScriptContextInstance(module, "Сценарий");
            scriptContext.AddProperty("ЭтотОбъект", scriptContext);
            scriptContext.InitOwnData();
            InitializeSDO(scriptContext);

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
            var scriptContext = new Machine.Contexts.UserScriptContextInstance(module.Module);
            InitializeSDO(scriptContext);
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
