/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts;
using OneScript.Sources;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine
{
    public class DependencyLoader
    {
        private readonly ScriptingEngine _engine;
        private readonly IRuntimeEnvironment _env;

        public DependencyLoader(ScriptingEngine engine)
        {
            _engine = engine;
            _env = engine.Environment;
        }

        public void AddClass(ICodeSource source, string className)
        {
            
        }

        public void AddModule(ICodeSource source, string moduleName)
        {
            
        }
    }
}