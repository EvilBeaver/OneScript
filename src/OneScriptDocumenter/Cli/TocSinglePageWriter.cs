/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScriptDocumenter.Model;
using OneScriptDocumenter.Secondary;

namespace OneScriptDocumenter.Cli
{
    public class TocSinglePageWriter : BaseModelVisitor, IDisposable
    {
        private readonly ReferenceFactory _referenceFactory;
        protected readonly MarkdownWriter _writer;

        private int _level = 0;

        public TocSinglePageWriter(string filename, ReferenceFactory referenceFactory)
        {
            _referenceFactory = referenceFactory;
            _writer = MarkdownWriter.OpenFile(filename);
        }
        
        public void Visit(SyntaxGroup model)
        {
            _writer.Header1(model.Title);
            
            VisitChildItems(model);
        }

        protected override void VisitChildItems(SyntaxGroup group)
        {
            _writer.BeginList();
            base.VisitChildItems(group);
            _writer.EndList();
        }

        protected override void VisitSyntaxGroup(SyntaxGroup model)
        {
            if (model.Page != null)
            {
                var linkTarget = _referenceFactory.BaseUrl + model.Page;
                _writer.ListItem(MarkdownReferenceResolver.MarkdownLink(linkTarget, model.Title));
            }
            else if (model.Document == null)
            {
                // Это какая-то битая ссылка из toc
                _writer.ListItem(MarkdownReferenceResolver.MarkdownLink("", model.Title));
            }
            
            base.VisitSyntaxGroup(model);
        }

        protected override void VisitDocument(IDocument document)
        {
            var linkTarget = _referenceFactory.CreateReference(document.Owner);
            _writer.ListItem(MarkdownReferenceResolver.MarkdownLink(linkTarget, document.Title));
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}