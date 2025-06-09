/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.IO;
using OneScriptDocumenter.Model;
using OneScriptDocumenter.Secondary;

namespace OneScriptDocumenter.Cli
{
    public class TocPagesGenerator : BaseModelVisitor
    {
        private readonly string _baseDir;
        private readonly ReferenceFactory _referenceFactory;

        public TocPagesGenerator(string baseDir, ReferenceFactory referenceFactory)
        {
            _baseDir = baseDir;
            _referenceFactory = referenceFactory;
        }

        protected override void VisitSyntaxGroup(SyntaxGroup model)
        {
            if (model.Page != null)
            {
                using var tocWriter = new TocSinglePageWriter(Path.Combine(_baseDir, model.Page + ".md"), _referenceFactory);
                tocWriter.Visit(model);
            }
            
            VisitChildItems(model);
        }
    }
}