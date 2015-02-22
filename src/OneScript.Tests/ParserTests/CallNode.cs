using OneScript.Compiler;

namespace OneScript.Tests
{
    class CallNode : TestASTNodeBase
    {

        string name;
        IASTNode[] arguments;

        public CallNode(string name, IASTNode[] args)
        {
            this.name = name;
            this.arguments = args;
        }

        protected override bool EqualsInternal(IASTNode other)
        {
            var callNode = other as CallNode;
            bool argIsOk = callNode.arguments.Length == this.arguments.Length;
            if (argIsOk)
                for (int i = 0; i < callNode.arguments.Length; i++)
                {
                    if (this.arguments[i] == null)
                    {
                        argIsOk = argIsOk && callNode.arguments[i] == null;
                    }
                    else
                    {
                        argIsOk = argIsOk && ((TestASTNodeBase)(this.arguments[i])).Equals(callNode.arguments[i]);
                    }
                    if (!argIsOk)
                        break;
                }

            return argIsOk && this.name == callNode.name;
        }
    }
}