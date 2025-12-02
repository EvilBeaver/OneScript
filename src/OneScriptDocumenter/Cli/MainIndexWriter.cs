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
    public class MainIndexWriter : TocSinglePageWriter
    {
        private int _level = 0;
        
        public MainIndexWriter(string outputDir, ReferenceFactory referenceFactory) 
            : base(Path.Combine(outputDir, "index.md"), referenceFactory)
        {
        }

        public override void VisitModel(DocumentationModel model)
        {
            _writer.Header1("Оглавление");
            base.VisitModel(model);
            Dispose();
        }

        protected override void VisitSyntaxGroup(SyntaxGroup model)
        {
            if (_level == 0)
            {
                _writer.Header2(model.Title);
                _level++;
                base.VisitChildItems(model);
                _level--;
            }
            else
            {
                _level++;
                base.VisitSyntaxGroup(model);
                _level--;
            }
        }
    }
}