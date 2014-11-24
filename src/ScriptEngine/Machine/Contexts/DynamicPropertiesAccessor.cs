using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    public abstract class DynamicPropertiesAccessor : PropertyNameIndexAccessor, IReflectableContext
    {
        private DynamicPropertiesHolder _propHolder;
        
        public DynamicPropertiesAccessor()
        {
            _propHolder = new DynamicPropertiesHolder();
        }
 
        protected int RegisterProperty(string name)
        {
            return _propHolder.RegisterProperty(name);
        }

        protected void RemoveProperty(string name)
        {
            _propHolder.RemoveProperty(name);
        }

        protected void ReorderPropertyNumbers()
        {
            _propHolder.ReorderPropertyNumbers();
        }

        protected void ClearProperties()
        {
            _propHolder.ClearProperties();
        }

        protected IEnumerable<KeyValuePair<string, int>> GetProperties()
        {
            return _propHolder.GetProperties();
        }

        #region IRuntimeContextInstance Members

        public override bool IsIndexed
        {
            get { return true; }
        }

        public override int FindProperty(string name)
        {
            try
            {
                return _propHolder.GetPropertyNumber(name);
            }
            catch (KeyNotFoundException)
            {
                throw RuntimeException.PropNotFoundException(name);
            }
        }

        public override bool IsPropReadable(int propNum)
        {
            return true;
        }

        public override bool IsPropWritable(int propNum)
        {
            return true;
        }

        #endregion


        IEnumerable<VariableInfo> IReflectableContext.GetProperties()
        {
            var props = this.GetProperties();

            var result = new List<VariableInfo>();

            foreach (var prop in props)
            {
                result.Add(new VariableInfo()
                {
                    Identifier = prop.Key,
                    Index = prop.Value,
                    Type = SymbolType.ContextProperty
                });
            }

            return result;
        }

        public IEnumerable<MethodInfo> GetMethods()
        {
            throw new NotImplementedException();
        }
    }
}
