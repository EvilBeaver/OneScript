using System.Reflection;

namespace ScriptEngine.Machine.Contexts
{
    class ReflectedParamInfo : ParameterInfo
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

        public void SetOwner(MemberInfo owner)
        {
            MemberImpl = owner;
        }

        public void SetPosition(int index)
        {
            PositionImpl = index;
        }

    }
}