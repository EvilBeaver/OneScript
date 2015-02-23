
namespace OneScript.Core
{
    public struct MethodParameter
    {
        public bool IsOptional;
        public bool IsByValue;
        public IValue DefaultValue;
    }
}