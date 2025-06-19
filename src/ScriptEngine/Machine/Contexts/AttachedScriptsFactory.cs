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
using OneScript.Sources;
using System.Security.Cryptography;
using OneScript.Commons;
using OneScript.Compilation;
using OneScript.Compilation.Binding;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.Types;
using ScriptEngine.Machine.Interfaces;

namespace ScriptEngine.Machine.Contexts
{
    public class AttachedScriptsFactory
    {
        private readonly Dictionary<string, IExecutableModule> _loadedModules;
        private readonly Dictionary<string, string> _fileHashes;
        
        private readonly ScriptingEngine _engine;
        private readonly IScriptCacheService _cacheService;

        internal AttachedScriptsFactory(ScriptingEngine engine)
        {
            _loadedModules = new Dictionary<string, IExecutableModule>(StringComparer.InvariantCultureIgnoreCase);
            _fileHashes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            _engine = engine;
            _cacheService = new ScriptCacheService();
            
            // Устанавливаем сериализатор модулей
            if (_cacheService is ScriptCacheService cache)
            {
                cache.SetModuleSerializer(new ScriptEngine.Compilation.StackRuntimeModuleSerializer());
                
                cache.CacheOperationLogged += (message) => 
                {
                    // Логируем операции кэша, если включен режим отладки
                    if (System.Environment.GetEnvironmentVariable("OS_CACHE_DEBUG") == "1")
                    {
                        Console.WriteLine($"[CACHE] {message}");
                    }
                };
            }
        }

        private ITypeManager TypeManager => _engine.TypeManager;
        
        /// <summary>
        /// Включить или отключить кэширование скомпилированных модулей
        /// </summary>
        /// <param name="enabled">true для включения кэширования</param>
        public void SetCachingEnabled(bool enabled)
        {
            if (_cacheService is ScriptCacheService cache)
            {
                cache.CachingEnabled = enabled;
            }
        }

        /// <summary>
        /// Получить сервис кэширования для настройки (например, подписки на события)
        /// </summary>
        public IScriptCacheService GetCacheService()
        {
            return _cacheService;
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

        public void AttachByPath(ICompilerFrontend compiler, string path, string typeName, IBslProcess process)
        {
            if (!Utils.IsValidIdentifier(typeName))
                throw RuntimeException.InvalidArgumentValue();

            var code = _engine.Loader.FromFile(path);
            
            ThrowIfTypeExist(typeName, code);

            CompileAndRegister(typeof(AttachedScriptsFactory), compiler, typeName, code, process);

        }

        public void AttachFromString(ICompilerFrontend compiler, string text, string typeName, IBslProcess process)
        {
            var code = _engine.Loader.FromString(text);
            ThrowIfTypeExist(typeName, code);
            
            CompileAndRegister(typeof(AttachedScriptsFactory), compiler, typeName, code, process);
        }

        public UserScriptContextInstance LoadFromPath(ICompilerFrontend compiler, string path, IBslProcess process)
        {
            return LoadFromPath(compiler, path, null, process);
        }

        public UserScriptContextInstance LoadFromPath(ICompilerFrontend compiler, string path,
            ExternalContextData externalContext, IBslProcess process)
        {
            var code = _engine.Loader.FromFile(path);
            return LoadAndCreate(compiler, code, externalContext, process);
        }

        public UserScriptContextInstance LoadFromString(ICompilerFrontend compiler, string text, IBslProcess process,
            ExternalContextData externalContext = null)
        {
            var code = _engine.Loader.FromString(text);
            return LoadAndCreate(compiler, code, externalContext, process);
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

        private void CompileAndRegister(Type type, ICompilerFrontend compiler, string typeName, SourceCode code, IBslProcess process)
        {
            if(_loadedModules.ContainsKey(typeName))
            {
                return;
            }

            var module = CompileModuleFromSource(compiler, code, null, process);
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
        
        private UserScriptContextInstance LoadAndCreate(ICompilerFrontend compiler, SourceCode code,
            ExternalContextData externalContext, IBslProcess process)
        {
            var module = CompileModuleFromSource(compiler, code, externalContext, process);
            return _engine.NewObject(module, process, externalContext);
        }

        public IExecutableModule CompileModuleFromSource(ICompilerFrontend compiler, SourceCode code, ExternalContextData externalContext, IBslProcess process)
        {
            var scope = compiler.FillSymbols(typeof(UserScriptContextInstance));
            if (externalContext != null)
            {
                foreach (var item in externalContext)
                {
                    scope.Variables.Add(new LocalVariableSymbol(item.Key, item.Value.GetType()));
                }
            }

            // Попытка загрузки из кэша только для файловых источников
            if (IsFileBasedSource(code) && externalContext == null)
            {
                if (_cacheService.TryLoadFromCache(code.Location, out var cachedModule))
                {
                    return cachedModule;
                }
            }

            // Компилируем обычным способом
            var module = compiler.Compile(code, process);

            // Сохраняем в кэш только для файловых источников без внешнего контекста
            if (IsFileBasedSource(code) && externalContext == null)
            {
                _cacheService.SaveToCache(code.Location, module);
            }

            return module;
        }

        private bool IsFileBasedSource(SourceCode code)
        {
            // Проверяем, что это файловый источник (не строка)
            // Файловые источники имеют путь в Location
            return !string.IsNullOrEmpty(code.Location) && 
                   !code.Location.Equals("<string>", StringComparison.OrdinalIgnoreCase) &&
                   System.IO.File.Exists(code.Location);
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
            UserScriptContextInstance newObj;
            if (module.GetInterface<IterableBslInterface>() != null)
            {
                newObj = new UserIterableContextInstance(module, type, arguments);
            }
            else
            {
                newObj = new UserScriptContextInstance(module, type, arguments);
            }

            newObj.InitOwnData();
            newObj.Initialize(context.CurrentProcess);

            return newObj;
        }

    }
}
