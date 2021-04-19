/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;

namespace ScriptEngine.Machine.Contexts
{
    public abstract class DynamicPropertiesAccessor : PropertyNameIndexAccessor
    {
        private readonly DynamicPropertiesHolder _propHolder;
        
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

        protected string GetPropertyName(int idx)
        {
            return _propHolder.GetPropertyName(idx);
        }

        protected virtual IEnumerable<KeyValuePair<string, int>> GetProperties()
        {
            return _propHolder.GetProperties();
        }

        #region IRuntimeContextInstance Members

        public override bool IsIndexed
        {
            get { return true; }
        }

        public override int GetPropCount()
        {
            return _propHolder.Count;
        }

        public override string GetPropName(int propNum)
        {
            return GetPropertyName(propNum);
        }

        public override int FindProperty(string name)
        {
            try
            {
                return _propHolder.GetPropertyNumber(name);
            }
            catch (KeyNotFoundException)
            {
                throw OldRuntimeException.PropNotFoundException(name);
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

    }
}
