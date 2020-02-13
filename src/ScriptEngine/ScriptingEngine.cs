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
            TypeManager.RegisterType("Сценарий", typeof(UserScriptContextInstance));
            
            GlobalsManager.Reset();
            AttachAssembly(System.Reflection.Assembly.GetExecutingAssembly());
            
            _scriptFactory = new ScriptSourceFactory();
            DirectiveResolvers = new DirectiveMultiResolver();

            SetupDirectiveResolution();
        }

        private void SetupDirectiveResolution()
        {
            var ignoreDirectiveResolver = new DirectiveIgnorer
            {
                {"Region", "Область"},
                {"EndRegion", "КонецОбласти"}
            };

            DirectiveResolvers.Add(ignoreDirectiveResolver);
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

        public void AttachExternalAssembly(System.Reflection.Assembly asm, RuntimeEnvironment globalEnvironment)
        {
            ContextDiscoverer.DiscoverClasses(asm);

            var lastCount = globalEnvironment.AttachedContexts.Count();
            ContextDiscoverer.DiscoverGlobalContexts(globalEnvironment, asm);

            var newCount = globalEnvironment.AttachedContexts.Count();
            while (lastCount < newCount)
            {
                _machine.AttachContext(globalEnvironment.AttachedContexts[lastCount]);
                ++lastCount;
            }
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

        public IList<IDirectiveResolver> DirectiveResolvers { get; }

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
            cs.DirectiveResolver = (IDirectiveResolver)DirectiveResolvers;
            return cs;
        }
        
        public IRuntimeContextInstance NewObject(LoadedModule module, ExternalContextData externalContext = null)
        {
            var scriptContext = CreateUninitializedSDO(module, externalContext);
            InitializeSDO(scriptContext);

            return scriptContext;
        }

        public ScriptDrivenObject CreateUninitializedSDO(LoadedModule module, ExternalContextData externalContext = null)
        {
            var scriptContext = new UserScriptContextInstance(module);
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

        public LoadedModule LoadModuleImage(ModuleImage moduleImage)
        {
            return new LoadedModule(moduleImage);
        }

        public void InitializeSDO(ScriptDrivenObject sdo)
        {
            sdo.Initialize();
        }

        public void ExecuteModule(LoadedModule module)
        {
            var scriptContext = new UserScriptContextInstance(module);
            InitializeSDO(scriptContext);
        }

        public MachineInstance Machine => _machine;

        public AttachedScriptsFactory AttachedScriptsFactory => _attachedScriptsFactory;

        public IDebugController DebugController
        {
            get => _debugController;
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
    }
}
