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
using ScriptEngine.Hosting;

namespace ScriptEngine
{
    public class OneScriptCoreOptions
    {
        private const string FILE_READER_ENCODING = "encoding.script";
        private const string SYSTEM_LANGUAGE_KEY = "SystemLanguage";
        private const string PREPROCESSOR_DEFINITIONS_KEY = "preprocessor.define";

        public OneScriptCoreOptions(KeyValueConfig config)
        {
            SystemLanguage = config[SYSTEM_LANGUAGE_KEY];
            FileReaderEncoding = SetupEncoding(config[FILE_READER_ENCODING]);
            PreprocessorDefinitions = SetupDefinitions(config[PREPROCESSOR_DEFINITIONS_KEY]);
        }

        public IEnumerable<string> PreprocessorDefinitions { get; set; }

        private IEnumerable<string> SetupDefinitions(string s)
        {
            return s?.Split(',') ?? new string[0];
        }

        private Encoding SetupEncoding(string openerEncoding)
        {
            if (string.IsNullOrWhiteSpace(openerEncoding)) 
                return Encoding.UTF8;
            
            if (StringComparer.InvariantCultureIgnoreCase.Compare(openerEncoding, "default") == 0)
                return FileOpener.SystemSpecificEncoding();
            else
                return Encoding.GetEncoding(openerEncoding);
        }

        public string SystemLanguage { get; set; }

        public Encoding FileReaderEncoding { get; set; }
    }
}