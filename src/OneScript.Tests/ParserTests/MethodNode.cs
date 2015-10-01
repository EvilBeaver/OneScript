using OneScript.Language;
using System.Collections.Generic;
using System.Linq;

namespace OneScript.Tests
{
    class MethodNode : TestASTNodeBase, IASTMethodDefinitionNode
    {
        public CodeBatchNode _body;
        public string _name;
        public bool _isExported;
        public bool _isFunction;

        protected override bool EqualsInternal(IASTNode other)
        {
            var otherMethod = other as MethodNode;
            if (otherMethod == null)
                return false;

            if (otherMethod._name != this._name)
                return false;

            if (otherMethod.Parameters.SequenceEqual(this.Parameters))
                return false;

            if (otherMethod._body == null && _body != null)
                return false;

            if (otherMethod._body != null && !otherMethod._body.Equals(_body))
                return false;

            return true;
        }

        public string Identifier
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public bool IsFunction
        {
            get
            {
                return _isFunction;
            }
            set
            {
                _isFunction = value;
            }
        }

        public bool IsExported
        {
            get
            {
                return _isExported;
            }
            set
            {
                _isExported = value;
            }
        }

        public ASTMethodParameter[] Parameters { get; set; }
        
        public IASTNode Body
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }
    }
}