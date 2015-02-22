using OneScript.Compiler;
using System;
using System.Collections.Generic;

namespace OneScript.Runtime
{
    class BCodeMethodNode : IASTNode
    {

        private List<ASTMethodParameter> _params;

        public BCodeMethodNode()
        {
            _params = new List<ASTMethodParameter>();

        }

        public int EntryPoint { get; set; }

        public string Name { get; set; }

        public bool IsFunction { get; set; }

        public bool IsExported { get; set; }

        public IList<ASTMethodParameter> Parameters
        {
            get
            {
                return _params;
            }
            set
            {
                _params = new List<ASTMethodParameter>(value);
            }
        }

        public IASTNode Body
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}