/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.IO;
using OneScript.Commons;
using OneScriptDocumenter.Model;
using OneScriptDocumenter.Secondary;

namespace OneScriptDocumenter.Cli
{
    public class MarkdownGenerator
    {
        private readonly DocumentationModel _documentation;
        private readonly ReferenceFactory _referenceFactory;

        public MarkdownGenerator(DocumentationModel documentation, ReferenceFactory referenceFactory)
        {
            _documentation = documentation;
            _referenceFactory = referenceFactory;
        }

        public void Write(string outputDir)
        {
            ConsoleLogger.Info("Generating markdown output");

            var walkers = new BaseModelVisitor[]
            {
                new MainIndexWriter(outputDir, _referenceFactory),
                new TocPagesGenerator(outputDir, _referenceFactory),
                new DocumentWriter(outputDir),
                new VitepressSidebarGenerator(Path.Combine(outputDir, "vitepress-toc.json"), _referenceFactory)
            };
            
            walkers.ForEach(w => w.VisitModel(_documentation));
        }
    }
}