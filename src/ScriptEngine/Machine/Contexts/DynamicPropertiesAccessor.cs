/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Values;

namespace ScriptEngine.Machine.Contexts
{
    public abstract class DynamicPropertiesAccessor : PropertyNameIndexAccessor
    {
        private readonly DynamicPropertiesHolder _propHolder;
        
        public DynamicPropertiesAccessor()
        {
            _propHolder = new DynamicPropertiesHolder(InfoFactory);
        }

        private BslPropertyInfo InfoFactory(int index, string identifier, bool canRead, bool canWrite)
        {
            return OnPropertyRegistration(index, identifier, canRead, canWrite);
        }
        
        protected virtual BslPropertyInfo OnPropertyRegistration(int index, string propertyName, bool canRead, bool canWrite)
        {
            return BslPropertyBuilder.Create()
                .Name(propertyName)
                .CanRead(canRead)
                .CanWrite(canWrite)
                .SetDispatchingIndex(index)
                .ReturnType(typeof(BslValue))
                .Build();
        }

        protected int RegisterProperty(string name, bool canRead = true, bool canWrite = true)
        {
            return _propHolder.RegisterProperty(name, canRead, canWrite);
        }
        
        protected int RegisterProperty(BslPropertyInfo propInfo)
        {
            return _propHolder.RegisterProperty(propInfo);
        }

        protected void RemoveProperty(string name)
        {
            _propHolder.RemoveProperty(name);
        }

        protected void ClearProperties()
        {
            _propHolder.ClearProperties();
        }

        protected string GetPropertyName(int idx)
        {
            return _propHolder.GetPropertyName(idx);
        }

        protected virtual IEnumerable<KeyValuePair<string, int>> GetDynamicProperties()
        {
            return _propHolder.GetProperties();
        }

        public override BslPropertyInfo GetPropertyInfo(int index)
        {
            return _propHolder[index];
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

        public override int GetPropertyNumber(string name)
        {
            try
            {
                return _propHolder.GetPropertyNumber(name);
            }
            catch (KeyNotFoundException)
            {
                throw PropertyAccessException.PropNotFoundException(name);
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
