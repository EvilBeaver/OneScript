using System.Collections.Generic;

namespace OneScriptDocumenter.Model
{
    class SyntaxCategory : AbstractSyntaxItem
    {
        public SyntaxCategory()
        {
            Children = new List<AbstractSyntaxItem>();
        }
    }
}
