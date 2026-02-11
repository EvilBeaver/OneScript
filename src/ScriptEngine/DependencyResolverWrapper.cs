using System;
using System.Collections.Generic;
using OneScript.Compilation;
using OneScript.Execution;
using OneScript.Sources;

namespace ScriptEngine
{
    public class DependencyResolverWrapper : IDependencyResolver
    {
        private readonly IDependencyResolver _inner;
        private readonly HashSet<string> _dependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public DependencyResolverWrapper(IDependencyResolver inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public IReadOnlyCollection<string> Dependencies => _dependencies;

        public void Reset()
        {
            _dependencies.Clear();
        }

        public PackageInfo Resolve(SourceCode module, string libraryName, IBslProcess process)
        {
            var resolved = _inner.Resolve(module, libraryName, process);
            if (resolved != null)
            {
                _dependencies.Add(resolved.Id);
            }

            return resolved;
        }

        public void Initialize(ScriptingEngine engine)
        {
            _inner.Initialize(engine);
        }
    }
}
