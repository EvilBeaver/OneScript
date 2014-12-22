using OneScript.Scripting.Compiler;
using System.Linq;

namespace OneScript.Tests
{
    class MethodNode : TestASTNodeBase
    {
        public CodeBatchNode _body;
        public string _name;
        public ASTMethodParameter[] _parameters;
        public bool _isExported;
        public bool _isFunction;

        public MethodNode(string name, bool isFunction)
        {
            _name = name;
            _isFunction = isFunction;
        }

        public void SetSignature(ASTMethodParameter[] parameters, bool exportFlag)
        {
            _parameters = parameters;
            _isExported = exportFlag;
        }

        protected override bool EqualsInternal(IASTNode other)
        {
            var otherMethod = other as MethodNode;
            if (otherMethod == null)
                return false;

            if (otherMethod._name != this._name)
                return false;

            if (otherMethod._parameters.SequenceEqual(this._parameters))
                return false;

            if (otherMethod._body == null && _body != null)
                return false;

            if (otherMethod._body != null && !otherMethod._body.Equals(_body))
                return false;

            return true;
        }
    }
}