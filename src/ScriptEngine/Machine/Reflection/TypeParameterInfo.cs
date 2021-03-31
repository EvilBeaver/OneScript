/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Reflection;

namespace ScriptEngine.Machine.Reflection
{
    public class TypeParameterInfo : ParameterInfo
    {
        public TypeParameterInfo(string name, Type type)
        {
            ClassImpl = type;
            NameImpl = name;
            
        }
        
        internal void SetOwner(MemberInfo owner)
        {
            MemberImpl = owner;
        }

        internal void SetPosition(int index)
        {
            PositionImpl = index;
        }
    }
}