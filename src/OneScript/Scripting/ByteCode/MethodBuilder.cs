using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting.ByteCode
{
    public class MethodBuilder : IBlockBuilder
    {
        private ModuleBuilder _builder;

        public MethodBuilder(ModuleBuilder builder)
        {
            this._builder = builder;
        }
        public void Build()
        {
            throw new NotImplementedException();
        }
    }
}
