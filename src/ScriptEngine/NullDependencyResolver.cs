/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language;
using OneScript.Sources;

namespace ScriptEngine
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class NullDependencyResolver : IDependencyResolver
    {
        public ExternalLibraryDef Resolve(SourceCode module, string libraryName)
        {
            return new ExternalLibraryDef(libraryName);
        }

        public void Initialize(ScriptingEngine engine)
        {
        }
    }
}