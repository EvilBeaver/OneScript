/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Reflection;
using OneScript.Contexts;
using OneScript.Language;
using OneScript.Native.Runtime;

namespace OneScript.Native.Compiler
{
    public class BuiltInFunctionsCache
    {
        private readonly IdentifiersTrie<MethodInfo> _cache = new IdentifiersTrie<MethodInfo>();

        public BuiltInFunctionsCache()
        {
            var publicMethods = typeof(BuiltInFunctions).GetMethods();
            foreach (var methodInfo in publicMethods)
            {
                var markup = methodInfo.GetCustomAttribute<ContextMethodAttribute>();
                if (markup == null)
                    continue;
                
                _cache.Add(markup.GetName(), methodInfo);
                if(markup.GetAlias() != null)
                    _cache.Add(markup.GetAlias(), methodInfo);
            }
        }

        public MethodInfo GetMethod(string name) => _cache[name];
    }
}