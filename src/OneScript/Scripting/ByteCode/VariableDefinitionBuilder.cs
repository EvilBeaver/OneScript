using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting.ByteCode
{
    public class VariableDefinitionBuilder : IBlockBuilder
    {
        private ModuleBuilder _builder;

        public VariableDefinitionBuilder(ModuleBuilder builder)
        {
            this._builder = builder;
        }
        public void Build()
        {
            var extractor = _builder.Extractor;
            var ctx = _builder.Context;

            do
            {
                extractor.Next();
                if (extractor.LastExtractedLexem.Type == LexemType.Identifier)
                {
                    ctx.DefineVariable(extractor.LastExtractedLexem.Content);
                }
                
                extractor.Next();
            } while (extractor.LastExtractedLexem.Token != Token.Semicolon);

        }
    }
}
