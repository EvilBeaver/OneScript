using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting.ByteCode
{
    public class AssignmentBuilder : IBlockBuilder
    {
        private ModuleBuilder _builder;

        public AssignmentBuilder(ModuleBuilder builder)
        {
            // TODO: Complete member initialization
            this._builder = builder;
        }
        void IBlockBuilder.Build()
        {
            throw new NotImplementedException();
        }
    }
}
