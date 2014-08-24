using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting.ByteCode
{
    public class CallBuilder : IBlockBuilder
    {
        private ModuleBuilder _builder;

        public CallBuilder(ModuleBuilder builder)
        {
            // TODO: Complete member initialization
            this._builder = builder;
        }
        public void Build()
        {
            throw new NotImplementedException();
        }
    }
}
