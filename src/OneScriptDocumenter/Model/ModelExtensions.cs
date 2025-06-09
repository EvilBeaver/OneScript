/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScriptDocumenter.Model
{
    public static class ModelExtensions
    {
        public static SyntaxGroup AddChildDocument(this SyntaxGroup group, IDocument document)
        {
            var docGroup = new SyntaxGroup
            {
                Document = document
            };
            
            group.Items.Add(docGroup);
            return docGroup;
        }
    }
}