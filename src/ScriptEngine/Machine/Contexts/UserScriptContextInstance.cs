using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine.Library;

namespace ScriptEngine.Machine.Contexts
{
    public class UserScriptContextInstance : ScriptDrivenObject
    {
        LoadedModule _module;
        
        internal UserScriptContextInstance(LoadedModule module) : base(module)
        {
            _module = module;
        }

        internal UserScriptContextInstance(LoadedModule module, string asObjectOfType)
            : base(module)
        {
            DefineType(TypeManager.GetTypeByName(asObjectOfType));
            _module = module;
        }

        protected override int GetMethodCount()
        {
            return 0;
        }

        protected override int GetVariableCount()
        {
            return 0;
        }

        protected override void UpdateState()
        {
        }

        #region IReflectableContext Members

        public override IEnumerable<VariableInfo> GetProperties()
        {
            foreach (var item in _module.ExportedProperies)
            {
                var vi = new VariableInfo();
                vi.Identifier = item.SymbolicName;
                vi.Index = item.Index;
                vi.Type = SymbolType.ContextProperty;
                
                yield return vi;
            }
        }

        public override IEnumerable<MethodInfo> GetMethods()
        {
            foreach (var item in _module.ExportedMethods)
            {
                yield return GetMethodInfo(item.Index);
            }
        }

        #endregion
    }
}
