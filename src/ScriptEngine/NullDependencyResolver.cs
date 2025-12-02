/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

#nullable enable
using OneScript.Compilation;
using OneScript.Execution;
using OneScript.Sources;

namespace ScriptEngine
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class NullDependencyResolver : IDependencyResolver
    {
        public PackageInfo? Resolve(SourceCode module, string libraryName, IBslProcess process)
        {
            return null;
        }

        public void Initialize(ScriptingEngine engine)
        {
        }
    }
}
