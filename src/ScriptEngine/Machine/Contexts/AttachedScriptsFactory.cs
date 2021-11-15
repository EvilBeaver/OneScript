/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using OneScript.Sources;
using System.Security.Cryptography;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Types;

namespace ScriptEngine.Machine.Contexts
{
    public class AttachedScriptsFactory
    {
        private readonly Dictionary<string, IExecutableModule> _loadedModules;
        private readonly Dictionary<string, string> _fileHashes;
        
        private readonly ScriptingEngine _engine;

        internal AttachedScriptsFactory(ScriptingEngine engine)
        {
            _loadedModules = new Dictionary<string, IExecutableModule>(StringComparer.InvariantCultureIgnoreCase);
            _fileHashes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            _engine = engine;
        }

        private ITypeManager TypeManager => _engine.TypeManager;
        
        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public void AttachByPath(ICompilerService compiler, string path, string typeName)
        {
            if (!Utils.IsValidIdentifier(typeName))
                throw RuntimeException.InvalidArgumentValue();

            var code = _engine.Loader.FromFile(path);
            
            ThrowIfTypeExist(typeName, code);

            CompileAndRegister(typeof(AttachedScriptsFactory), compiler, typeName, code);

        }

        public void AttachFromString(ICompilerService compiler, string text, string typeName)
        {
            var code = _engine.Loader.FromString(text);
            ThrowIfTypeExist(typeName, code);
            
            CompileAndRegister(typeof(AttachedScriptsFactory), compiler, typeName, code);
        }

        public IRuntimeContextInstance LoadFromPath(ICompilerService compiler, string path)
        {
            return LoadFromPath(compiler, path, null);
        }

        public IRuntimeContextInstance LoadFromPath(ICompilerService compiler, string path, ExternalContextData externalContext)
        {
            var code = _engine.Loader.FromFile(path);
            return LoadAndCreate(compiler, code, externalContext);
        }

        public IRuntimeContextInstance LoadFromString(ICompilerService compiler, string text, ExternalContextData externalContext = null)
        {
            var code = _engine.Loader.FromString(text);
            return LoadAndCreate(compiler, code, externalContext);
        }


        private void ThrowIfTypeExist(string typeName, SourceCode code)
        {
            if (TypeManager.IsKnownType(typeName) && _loadedModules.ContainsKey(typeName))
            {
                using (MD5 md5Hash = MD5.Create())
                {
                    string moduleCode = code.GetSourceCode();
                    string hash = GetMd5Hash(md5Hash, moduleCode);
                    string storedHash = _fileHashes[typeName];

                    StringComparer comparer = StringComparer.OrdinalIgnoreCase;
                    if(comparer.Compare(hash, storedHash) != 0)
                        throw new RuntimeException("Type «" + typeName + "» already registered");
                }

            }

        }

        private void CompileAndRegister(Type type, ICompilerService compiler, string typeName, SourceCode code)
        {
            if(_loadedModules.ContainsKey(typeName))
            {
                return;
            }

            var module = CompileModuleFromSource(compiler, code, null);
            _loadedModules.Add(typeName, module);
            using(var md5Hash = MD5.Create())
            {
                var hash = GetMd5Hash(md5Hash, code.GetSourceCode());
                _fileHashes.Add(typeName, hash);
            }

            TypeManager.RegisterType(typeName, default, type);

        }

        public void RegisterTypeModule(string typeName, IExecutableModule module)
        {
            if (_loadedModules.ContainsKey(typeName))
            {
                var alreadyLoadedSrc = (_loadedModules[typeName]).Source.Location;
                var currentSrc = module.Source.Location;

                if(alreadyLoadedSrc != currentSrc)
                    throw new RuntimeException("Type «" + typeName + "» already registered");

                return;
            }
            
            _loadedModules.Add(typeName, module);
            _engine.TypeManager.RegisterType(typeName, default, typeof(AttachedScriptsFactory));
        }
        
        public void LoadImageAndRegister(string typeName, ModuleImage module)
        {
            if (_loadedModules.ContainsKey(typeName))
            {
                var alreadyLoadedSrc = _loadedModules[typeName].Source.Location;
                var currentSrc = module.Source.Location;

                if(alreadyLoadedSrc != currentSrc)
                    throw new RuntimeException("Type «" + typeName + "» already registered");

                return;
            }

            throw new NotImplementedException("Deserialization of module not implemented");
            // var loadedModule = new LoadedModule(module);
            // _loadedModules.Add(typeName, loadedModule);
            //
            // _engine.TypeManager.RegisterType(typeName, default, typeof(AttachedScriptsFactory));

        }

        private IRuntimeContextInstance LoadAndCreate(ICompilerService compiler, SourceCode code, ExternalContextData externalContext)
        {
            var module = CompileModuleFromSource(compiler, code, externalContext);
            return _engine.NewObject(module, externalContext);
        }

        public IExecutableModule CompileModuleFromSource(ICompilerService compiler, SourceCode code, ExternalContextData externalContext)
        {
            UserScriptContextInstance.PrepareCompilation(compiler);
                
            if (externalContext != null)
            {
                foreach (var item in externalContext)
                {
                    compiler.DefineVariable(item.Key, null, SymbolType.ContextProperty);
                }
            }

            return compiler.Compile(code);
        }
        
        private static AttachedScriptsFactory _instance;

        static AttachedScriptsFactory()
        {
        }

        internal static void SetInstance(AttachedScriptsFactory factory)
        {
            _instance = factory;
        }

        public static IExecutableModule GetModuleOfType(string typeName)
        {
            return _instance._loadedModules[typeName];
        }

        [ScriptConstructor]
        public static UserScriptContextInstance ScriptFactory(TypeActivationContext context, IValue[] arguments)
        {
            var module = _instance._loadedModules[context.TypeName];

            var type = context.TypeManager.GetTypeByName(context.TypeName); 
            
            var newObj = new UserScriptContextInstance(module, type, arguments);
            newObj.InitOwnData();
            newObj.Initialize();

            return newObj;
        }

    }
}
