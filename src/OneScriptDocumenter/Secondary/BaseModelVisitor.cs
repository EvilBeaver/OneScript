/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScriptDocumenter.Model;

namespace OneScriptDocumenter.Secondary
{
    public abstract class BaseModelVisitor
    {
        public virtual void VisitModel(DocumentationModel model)
        {
            VisitItems(model.Items);
        }

        private void VisitItems(IEnumerable<SyntaxGroup> modelItems)
        {
            foreach (var syntaxGroup in modelItems)
            {
                VisitSyntaxGroup(syntaxGroup);
            }
        }

        protected virtual void VisitChildItems(SyntaxGroup group)
        {
            if (group.Items != null)
                VisitItems(group.Items);
        }

        protected virtual void VisitSyntaxGroup(SyntaxGroup model)
        {
            if (model.Document != null)
            {
                VisitDocument(model.Document);
            }
            
            VisitChildItems(model);
        }

        protected virtual void VisitDocument(IDocument document)
        {
            switch (document)
            {
                case GlobalContextModel model:
                    VisitGlobalContext(model);
                    break;
                case ClassModel model:
                    VisitClass(model);
                    break;
                case EnumModel model:
                    VisitEnum(model);
                    break;
            }
        }

        protected virtual void VisitClass(ClassModel model)
        {
        }

        protected virtual void VisitGlobalContext(GlobalContextModel model)
        {
        }

        protected virtual void VisitEnum(EnumModel model)
        {
        }
    }
}