﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneScript.Language.LexicalAnalysis;

namespace ScriptEngine.Compiler
{
    class CompiledCodeIndexer : ISourceCodeIndexer
    {
        public string GetCodeLine(int index)
        {
            return "<исходный код недоступен>";
        }
    }
}
