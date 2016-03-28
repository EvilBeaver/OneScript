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
using ScriptEngine.Compiler;

namespace ScriptEngine.Environment
{
    class ScriptSourceFactory : ICodeSourceFactory
    {
        public ScriptSourceFactory()
        {
            ReaderEncoding = Encoding.UTF8;
        }
        
        public ICodeSource FromString(string source)
        {
            return new StringBasedSource(source);
        }

        public ICodeSource FromFile(string path)
        {
            return new FileBasedSource(path, ReaderEncoding);
        }

        public Encoding ReaderEncoding { get; set; }
    }
}
