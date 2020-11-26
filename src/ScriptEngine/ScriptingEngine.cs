/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScriptEngine.Environment;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Compiler;
using ScriptEngine.Hosting;

namespace ScriptEngine
{
    public class ScriptingEngine : IDisposable
    {
        private readonly ICompilerServiceFactory _compilerFactory;
        
        private readonly ScriptSourceFactory _scriptFactory;
        private AttachedScriptsFactory _attachedScriptsFactory;
        private IDebugController _debugController;

        [Obsolete]
        public ScriptingEngine()
        {
            TypeManager.Initialize(new StandartTypeManager());
            TypeManager.RegisterType("Сценарий", typeof(UserScriptContextInstance));
            
            GlobalsManager.Reset();
            
            _scriptFactory = new ScriptSourceFactory();
            DirectiveResolvers = new DirectiveMultiResolver();
            ContextDiscoverer = new ContextDiscoverer(TypeManager.Instance, GlobalsManager.Instance);
            _compilerFactory = new AstBasedCompilerFactory();
            
            AttachAssembly(System.Reflection.Assembly.GetExecutingAssembly());
        }

        public ScriptingEngine(ITypeManager types, IGlobalsManager globals, RuntimeEnvironment env, ICompilerServiceFactory compilerFactory)
        {
            _compilerFactory = compilerFactory;
            // FIXME: Пока потребители не отказались от статических инстансов, они будут жить и здесь
            TypeManager.Initialize(types);
            TypeManager.RegisterType("Сценарий", typeof(UserScriptContextInstance));

            GlobalsManager.Instance = globals;
            Environment = env;
            
            _scriptFactory = new ScriptSourceFactory();
            DirectiveResolvers = new DirectiveMultiResolver();
            ContextDiscoverer = new ContextDiscoverer(types, globals);
        }
        
        private ContextDiscoverer ContextDiscoverer { get; }
        
        public RuntimeEnvironment Environment { get; set; }
        
        public CodeGenerationFlags ProduceExtraCode { get; set; }

        public void AttachAssembly(System.Reflection.Assembly asm)
        {
            ContextDiscoverer.DiscoverClasses(asm);
        }

        public void AttachAssembly(System.Reflection.Assembly asm, RuntimeEnvironment globalEnvironment, Predicate<Type> filter = null)
        {
            ContextDiscoverer.DiscoverClasses(asm, filter);
            ContextDiscoverer.DiscoverGlobalContexts(globalEnvironment, asm, filter);
        }

        public void AttachExternalAssembly(System.Reflection.Assembly asm, RuntimeEnvironment globalEnvironment)
        {
            ContextDiscoverer.DiscoverClasses(asm);

            var lastCount = globalEnvironment.AttachedContexts.Count();
            ContextDiscoverer.DiscoverGlobalContexts(globalEnvironment, asm);

            var newCount = globalEnvironment.AttachedContexts.Count();
            while (lastCount < newCount)
            {
                MachineInstance.Current.AttachContext(globalEnvironment.AttachedContexts[lastCount]);
                ++lastCount;
            }
        }

        public void Initialize()
        {
            SetDefaultEnvironmentIfNeeded();

            UpdateContexts();

            _attachedScriptsFactory = new AttachedScriptsFactory(this);
            AttachedScriptsFactory.SetInstance(_attachedScriptsFactory);
        }

        public void UpdateContexts()
        {
            Environment.LoadMemory(MachineInstance.Current);
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

        [Obsolete]
        public IList<IDirectiveResolver> DirectiveResolvers { get; }

        public ICompilerService GetCompilerService()
        {
            var cs = _compilerFactory.CreateInstance(Environment.SymbolsContext);
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
            //cs.DirectiveResolver = (IDirectiveResolver)DirectiveResolvers;
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
        
        public Task InitializeSDOAsync(ScriptDrivenObject sdo)
        {
            return sdo.InitializeAsync();
        }

        public void ExecuteModule(LoadedModule module)
        {
            var scriptContext = new UserScriptContextInstance(module);
            InitializeSDO(scriptContext);
        }

        public AttachedScriptsFactory AttachedScriptsFactory => _attachedScriptsFactory;

        public IDebugController DebugController
        {
            get => _debugController;
            set
            {
                _debugController = value;
                if (value != null)
                {
                    ProduceExtraCode = CodeGenerationFlags.DebugCode;
                    MachineInstance.Current.SetDebugMode(_debugController.BreakpointManager);
                }
            }
        }

        public void SetCodeStatisticsCollector(ICodeStatCollector collector)
        {
            ProduceExtraCode = CodeGenerationFlags.CodeStatistics;
            MachineInstance.Current.SetCodeStatisticsCollector(collector);
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
