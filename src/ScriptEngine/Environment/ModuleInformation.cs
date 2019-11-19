/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneScript.Language.LexicalAnalysis;

namespace ScriptEngine.Environment
{
    [Serializable]
    public class ModuleInformation
    {
        [NonSerialized]
        private ISourceCodeIndexer _codeIndexer;

        public string ModuleName { get; set; }

        public ISourceCodeIndexer CodeIndexer
        {
            get => _codeIndexer;
            set => _codeIndexer = value;
        }

        public string Origin { get; set; }

        public override string ToString()
        {
            return ModuleName;
        }
    }
}
