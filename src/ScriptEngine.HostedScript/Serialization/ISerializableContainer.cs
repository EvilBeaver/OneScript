/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript.Serialization
{
    public interface ISerializableContext<TVal> : ISerializableContext where TVal : IValue
    {
        new TVal GetValue();

        void SetValue(TVal value);
    }

    public interface ISerializableContext
    {
        bool CanProcess(Type type);
        
        IValue GetValue();

        void SetValue(IValue value);
    }
}