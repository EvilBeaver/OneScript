/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript.Serialization.Containers
{
    public abstract class SerializableContextBase<TVal> : ISerializableContext<TVal> where TVal: IValue
    {
        public bool CanProcess(Type type)
        {
            var valType = typeof(TVal);
            return valType == type || valType.IsAssignableFrom(type);
        }

        public abstract TVal GetValue();

        public abstract void SetValue(TVal value);

        IValue ISerializableContext.GetValue() 
            => GetValue();

        void ISerializableContext.SetValue(IValue value) 
            => SetValue((TVal)value);
    }

    public abstract class SerializableContextBase : ISerializableContext
    {
        public abstract bool CanProcess(Type type);

        public abstract IValue GetValue();

        public abstract void SetValue(IValue value);
    }
}