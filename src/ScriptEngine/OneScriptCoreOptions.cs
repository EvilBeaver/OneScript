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

        public OneScriptCoreOptions(KeyValueConfig config)
        {
            SystemLanguage = config[SYSTEM_LANGUAGE_KEY];
            FileReaderEncoding = SetupEncoding(config[FILE_READER_ENCODING]);
            PreprocessorDefinitions = SetupDefinitions(config[PREPROCESSOR_DEFINITIONS_KEY]);
            UseNativeAsDefaultRuntime = SetupDefaultRuntime(config[DEFAULT_RUNTIME_KEY]);
        }

        public string SystemLanguage { get; }

        public Encoding FileReaderEncoding { get; }

        public bool UseNativeAsDefaultRuntime { get; }
        
        public IEnumerable<string> PreprocessorDefinitions { get; set; }

        private static IEnumerable<string> SetupDefinitions(string s)
        {
            return s?.Split(',') ?? Array.Empty<string>();
        }

        private static Encoding SetupEncoding(string openerEncoding)
        {
            if (string.IsNullOrWhiteSpace(openerEncoding)) 
                return Encoding.UTF8;
            
            if (StringComparer.InvariantCultureIgnoreCase.Compare(openerEncoding, "default") == 0)
                return FileOpener.SystemSpecificEncoding();
            else
                return Encoding.GetEncoding(openerEncoding);
        }
        
        private static bool SetupDefaultRuntime(string runtimeId)
        {
            return runtimeId == NativeRuntimeAnnotationHandler.NativeDirectiveName;
        }
    }
}