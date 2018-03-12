/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
//#if !__MonoCS__
using System.Reflection;

namespace ScriptEngine.Machine.Contexts
{
    public class ReflectedParamInfo : ParameterInfo
    {
        public ReflectedParamInfo(string name, bool isByVal)
        {
            NameImpl = name;
            AttrsImpl = ParameterAttributes.In;
            if (!isByVal)
            {
                AttrsImpl |= ParameterAttributes.Out;
            }

            ClassImpl = typeof(IValue);

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
//#endif