/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Text;
using OneScript.Commons;
using OneScript.Native.Compiler;
using ScriptEngine.Hosting;

namespace ScriptEngine
{
    public class OneScriptCoreOptions
    {
        private const string FILE_READER_ENCODING = "encoding.script";
        private const string SYSTEM_LANGUAGE_KEY = "SystemLanguage";
        private const string PREPROCESSOR_DEFINITIONS_KEY = "preprocessor.define";
        private const string DEFAULT_RUNTIME_KEY = "runtime.default";
        private const string EXPLICIT_IMPORT = "lang.explicitImports";

        public OneScriptCoreOptions(KeyValueConfig config)
        {
            SystemLanguage = config[SYSTEM_LANGUAGE_KEY];
            FileReaderEncoding = SetupEncoding(config[FILE_READER_ENCODING]);
            PreprocessorDefinitions = SetupDefinitions(config[PREPROCESSOR_DEFINITIONS_KEY]);
            UseNativeAsDefaultRuntime = SetupDefaultRuntime(config[DEFAULT_RUNTIME_KEY]);
            ExplicitImports = SetupExplicitImports(config[EXPLICIT_IMPORT]);
        }

        public string SystemLanguage { get; }

        public Encoding FileReaderEncoding { get; }

        public bool UseNativeAsDefaultRuntime { get; }
        
        public IEnumerable<string> PreprocessorDefinitions { get; }
        
        public ExplicitImportsBehavior ExplicitImports { get; }

        private static IEnumerable<string> SetupDefinitions(string s)
        {
            return s?.Split(',') ?? Array.Empty<string>();
        }

        private static Encoding SetupEncoding(string openerEncoding)
        {
            if (string.IsNullOrWhiteSpace(openerEncoding)) 
                return Encoding.UTF8;

            return StringComparer.InvariantCultureIgnoreCase.Compare(openerEncoding, "default") == 0 ? 
                FileOpener.SystemSpecificEncoding() : 
                Encoding.GetEncoding(openerEncoding);
        }
        
        private static bool SetupDefaultRuntime(string runtimeId)
        {
            return runtimeId == NativeRuntimeAnnotationHandler.NativeDirectiveName;
        }

        private static ExplicitImportsBehavior SetupExplicitImports(string keyValue)
        {
            switch (keyValue)
            {
                case "on":
                    return ExplicitImportsBehavior.Enabled;
                case "off":
                    return ExplicitImportsBehavior.Disabled;
                case "warn":
                    return ExplicitImportsBehavior.Warn;
                case "dev":
                case null:
                    return ExplicitImportsBehavior.Development;
                default:
                    SystemLogger.Write($"Unknown value for {EXPLICIT_IMPORT}: {keyValue}");
                    return ExplicitImportsBehavior.Warn;
            }
        }
    }
}