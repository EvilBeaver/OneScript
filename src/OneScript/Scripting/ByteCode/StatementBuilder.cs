using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting.ByteCode
{
    public class StatementBuilder : IBlockBuilder
    {
        private ModuleBuilder _builder;

        public StatementBuilder(ModuleBuilder builder)
        {
            this._builder = builder;
        }
        public void Build()
        {
            throw new NotImplementedException();
        }
    }
}
