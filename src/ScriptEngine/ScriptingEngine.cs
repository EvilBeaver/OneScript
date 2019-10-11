/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;

using ScriptEngine.Environment;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Compiler;

namespace ScriptEngine
{
    public class ScriptingEngine : IDisposable
    {
        private readonly MachineInstance _machine;
        private readonly ScriptSourceFactory _scriptFactory;
        private AttachedScriptsFactory _attachedScriptsFactory;
        private IDebugController _debugController;

        public ScriptingEngine()
        {
            _machine = MachineInstance.Current;

            TypeManager.Initialize(new StandartTypeManager());
            GlobalsManager.Reset();
            ContextDiscoverer.DiscoverClasses(System.Reflection.Assembly.GetExecutingAssembly());
            
            _scriptFactory = new ScriptSourceFactory();
        }

        public CodeGenerationFlags ProduceExtraCode { get; set; }

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

        public void Initialize()
        {
            SetDefaultEnvironmentIfNeeded();

            UpdateContexts();

            _attachedScriptsFactory = new AttachedScriptsFactory(this);
            AttachedScriptsFactory.SetInstance(_attachedScriptsFactory);
        }

        public void UpdateContexts()
        {
            Environment.LoadMemory(_machine);
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
            switch (System.Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                    cs.DefinePreprocessorValue("Linux");
                    break;
                case PlatformID.MacOSX:
                    cs.DefinePreprocessorValue("MacOS");
                    break;
                case PlatformID.Win32NT:
                    cs.DefinePreprocessorValue("Windows");
                    break;
            }
            
            cs.ProduceExtraCode = ProduceExtraCode;
            cs.DirectiveResolver = DirectiveResolver;
            return cs;
        }
        
        public IRuntimeContextInstance NewObject(LoadedModule module, ExternalContextData externalContext = null)
        {
            var scriptContext = CreateUninitializedSDO(module, externalContext);
            InitializeSDO(scriptContext);

            return scriptContext;
        }

        private ScriptDrivenObject CreateUninitializedSDO(LoadedModule module, ExternalContextData externalContext = null)
        {
            var scriptContext = new Machine.Contexts.UserScriptContextInstance(module);
            scriptContext.AddProperty("ЭтотОбъект", "ThisObject", scriptContext);
            if (externalContext != null)
            {
                foreach (var item in externalContext)
                {
                    scriptContext.AddProperty(item.Key, item.Value);
                }
            }

            scriptContext.InitOwnData();
            return scriptContext;
        }

        [Obsolete]
        public IRuntimeContextInstance NewObject(LoadedModuleHandle module)
        {
            return NewObject(module.Module); 
        }

        [Obsolete]
        public IRuntimeContextInstance NewObject(LoadedModuleHandle module, ExternalContextData externalContext)
        {
            return NewObject(module.Module, externalContext);
        }

        [Obsolete]
        public LoadedModuleHandle LoadModuleImage(ScriptModuleHandle moduleImage)
        {
            var handle = new LoadedModuleHandle();
            handle.Module = new LoadedModule(moduleImage.Module);
            return handle;
        }

        public LoadedModule LoadModuleImage(ModuleImage moduleImage)
        {
            return new LoadedModule(moduleImage);
        }

        public void InitializeSDO(ScriptDrivenObject sdo)
        {
            sdo.Initialize();
        }

        [Obsolete]
        public void ExecuteModule(LoadedModuleHandle module)
        {
            ExecuteModule(module.Module);
        }

        public void ExecuteModule(LoadedModule module)
        {
            var scriptContext = new Machine.Contexts.UserScriptContextInstance(module);
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

        public IDebugController DebugController
        {
            get
            {
                return _debugController;
            }
            set
            {
                _debugController = value;
                ProduceExtraCode = CodeGenerationFlags.DebugCode;
                _machine.SetDebugMode(_debugController);
            }
        }

        public void SetCodeStatisticsCollector(ICodeStatCollector collector)
        {
            ProduceExtraCode = CodeGenerationFlags.CodeStatistics;
            _machine.SetCodeStatisticsCollector(collector);
        }

        #region IDisposable Members

        public void Dispose()
        {
            AttachedScriptsFactory.SetInstance(null);
            GlobalsManager.Reset();
        }

        #endregion

        public void CompileEnvironmentModules(RuntimeEnvironment env)
        {
            var scripts = env.GetUserAddedScripts().Where(x => x.Type == UserAddedScriptType.Module && env.GetGlobalProperty(x.Symbol) == null)
                             .ToArray();

            if (scripts.Length > 0)
            {
                var loadedObjects = new ScriptDrivenObject[scripts.Length];
                for(var i = 0; i < scripts.Length; i++)
                {
                    var script = scripts[i];
                    var loaded = LoadModuleImage(script.Image);

                    var instance = CreateUninitializedSDO(loaded);
                    env.SetGlobalProperty(script.Symbol, instance);
                    loadedObjects[i] = instance;
                }

                foreach (var instance in loadedObjects)
                {
                    InitializeSDO(instance);
                }
            }
        }
        
    }
}
