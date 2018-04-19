/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ScriptEngine.Environment;

namespace OneScript.ASPNETHandler
{
    class AspEntryScriptSrc : ICodeSource
    {
        string _sourceDescription;

        public string Code
        {
            get { return ""; }
        }
        public AspEntryScriptSrc(string sourceDescription)
        {
            _sourceDescription = sourceDescription;
        }
        public string SourceDescription
        {
            get { return _sourceDescription; }
        }
    }
}
