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
