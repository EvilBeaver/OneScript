using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting.ByteCode
{
    public class MethodSectionBuilder : IBlockBuilder
    {
        private ModuleBuilder _builder;

        public MethodSectionBuilder(ModuleBuilder builder)
        {
            this._builder = builder;
        }
        public void Build()
        {
            throw new NotImplementedException();
        }
    }
}
