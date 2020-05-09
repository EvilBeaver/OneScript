namespace ScriptEngine.Machine.Rcw
{
    public class RcwMethodMetadata : RcwMemberMetadata
    {
        public bool IsFunction { get; }

        public RcwMethodMetadata(string name, int dispId, bool isFunc) : base(name, dispId)
        {
            IsFunction = isFunc;
        }
    }

    public class RcwPropertyMetadata : RcwMemberMetadata
    {
        public bool IsReadable { get; internal set; }

        public bool IsWritable { get; internal set; }

        public RcwPropertyMetadata(string name, int dispId) : base(name, dispId)
        {

        }
    }

    public abstract class RcwMemberMetadata
    {
        public int DispatchId { get; }

        public string Name { get; }

        protected RcwMemberMetadata(string name, int dispId)
        {
            Name = name;
            DispatchId = dispId;
        }
    }
}