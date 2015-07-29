/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript.Library.Http
{
    interface IHttpRequestBody : IDisposable
    {
        IValue GetAsString();
        IValue GetAsBinary();
        IValue GetAsFilename();

        Stream GetDataStream();
    }

}
