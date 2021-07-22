/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Text;
using OneScript.Sources;

namespace ScriptEngine
{
    public class ScriptSourceFactory
    {
        public ScriptSourceFactory()
        {
            ReaderEncoding = Encoding.UTF8;
        }
        
        public SourceCode FromString(string code)
        {
            return SourceCodeBuilder.Create()
                .FromString(code)
                .Build();
        }

        public SourceCode FromFile(string path)
        {
            return SourceCodeBuilder
                .Create()
                .FromFile(path, ReaderEncoding)
                .Build();
        }

        public Encoding ReaderEncoding { get; set; }
    }
}
