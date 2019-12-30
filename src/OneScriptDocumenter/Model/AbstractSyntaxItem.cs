using System.Collections.Generic;

namespace OneScriptDocumenter.Model
{
    abstract class AbstractSyntaxItem
    {
        public MultilangString Caption { get; set; }
        public MultilangString Description { get; set; }

        public IList<AbstractSyntaxItem> Children { get; protected set; }
    }
}
