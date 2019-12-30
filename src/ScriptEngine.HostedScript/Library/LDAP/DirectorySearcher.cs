using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.DirectoryServices;

namespace ScriptEngine.HostedScript.Library.LDAP
{
    [ContextClass("ПоискВКаталоге", "DirectorySearcher")]
    class DirectorySearcherImpl : AutoContext<DirectorySearcherImpl>
    {
    }
}
