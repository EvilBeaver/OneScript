/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Environment;
using System.Security.Cryptography;

namespace ScriptEngine.Machine.Contexts
{
    public class AttachedScriptsFactory
    {
        private Dictionary<string, LoadedModule> _loadedModules;
        private Dictionary<string, string> _fileHashes;
        
        private ScriptingEngine _engine;

        internal AttachedScriptsFactory(ScriptingEngine engine)
        {
            _loadedModules = new Dictionary<string, LoadedModule>(StringComparer.InvariantCultureIgnoreCase);
            _fileHashes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            _engine = engine;
        }

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

        public void AttachByPath(CompilerService compiler, string path, string typeName)
        {
            if (!Utils.IsValidIdentifier(typeName))
                throw RuntimeException.InvalidArgumentValue();

            var code = _engine.Loader.FromFile(path);
            
            ThrowIfTypeExist(typeName, code);

            LoadAndRegister(typeof(AttachedScriptsFactory), compiler, typeName, code);

        }

        public void AttachFromString(CompilerService compiler, string text, string typeName)
        {
            var code = _engine.Loader.FromString(text);
            ThrowIfTypeExist(typeName, code);
            
            LoadAndRegister(typeof(AttachedScriptsFactory), compiler, typeName, code);
        }

        public IRuntimeContextInstance LoadFromPath(CompilerService compiler, string path)
        {
            return LoadFromPath(compiler, path, null);
        }

        public IRuntimeContextInstance LoadFromPath(CompilerService compiler, string path, ExternalContextData externalContext)
        {
            var code = _engine.Loader.FromFile(path);
            return LoadAndCreate(compiler, code, externalContext);
        }

        public IRuntimeContextInstance LoadFromString(CompilerService compiler, string text)
        {
            var code = _engine.Loader.FromString(text);
            return LoadAndCreate(compiler, code, null);
        }


        private void ThrowIfTypeExist(string typeName, ICodeSource code)
        {
            if (TypeManager.IsKnownType(typeName) && _loadedModules.ContainsKey(typeName))
            {
                using (MD5 md5Hash = MD5.Create())
                {
                    string moduleCode = code.Code;
                    string hash = GetMd5Hash(md5Hash, moduleCode);
                    string storedHash = _fileHashes[typeName];

                    StringComparer comparer = StringComparer.OrdinalIgnoreCase;
                    if(comparer.Compare(hash, storedHash) != 0)
                        throw new RuntimeException("Type «" + typeName + "» already registered");
                }

            }

        }

        private void LoadAndRegister(Type type, CompilerService compiler, string typeName, Environment.ICodeSource code)
        {
            if(_loadedModules.ContainsKey(typeName))
            {
                return;
            }

            var moduleHandle = CreateModuleFromSource(compiler, code, null);
            var loadedHandle = _engine.LoadModuleImage(moduleHandle);

            _loadedModules.Add(typeName, loadedHandle.Module);
            using(var md5Hash = MD5.Create())
            {
                var hash = GetMd5Hash(md5Hash, code.Code);
                _fileHashes.Add(typeName, hash);
            }

            TypeManager.RegisterType(typeName, type);

        }

        public void LoadAndRegister(string typeName, ScriptModuleHandle moduleHandle)
        {
            if (_loadedModules.ContainsKey(typeName))
            {
                return;
            }

            var loadedHandle = _engine.LoadModuleImage(moduleHandle);
            _loadedModules.Add(typeName, loadedHandle.Module);
            
            TypeManager.RegisterType(typeName, typeof(AttachedScriptsFactory));

        }

        private IRuntimeContextInstance LoadAndCreate(CompilerService compiler, Environment.ICodeSource code, ExternalContextData externalContext)
        {
            var moduleHandle = CreateModuleFromSource(compiler, code, externalContext);
            var loadedHandle = _engine.LoadModuleImage(moduleHandle);
            return _engine.NewObject(loadedHandle.Module, externalContext);
        }

        public ScriptModuleHandle CreateModuleFromSource(CompilerService compiler, Environment.ICodeSource code, ExternalContextData externalContext)
        {
            compiler.DefineVariable("ЭтотОбъект", SymbolType.ContextProperty);
            if (externalContext != null)
            {
                foreach (var item in externalContext)
                {
                    compiler.DefineVariable(item.Key, SymbolType.ContextProperty);
                }
            }

            return compiler.CreateModule(code);
        }

        private static AttachedScriptsFactory _instance;

        static AttachedScriptsFactory()
        {
        }

        internal static void SetInstance(AttachedScriptsFactory factory)
        {
            _instance = factory;
        }

        public static void Dispose()
        {
            _instance = null;
        }

        [ScriptConstructor(ParametrizeWithClassName = true)]
        public static IRuntimeContextInstance ScriptFactory(string typeName, IValue[] arguments)
        {
            var module = _instance._loadedModules[typeName];

            var newObj = new UserScriptContextInstance(module, typeName);
            newObj.AddProperty("ЭтотОбъект", newObj);
            newObj.InitOwnData();
            newObj.Initialize(_instance._engine.Machine);

            return newObj;
        }

    }
}
