/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;

namespace OneScriptDocumenter.Model
{
    public class ClassModel : TypeModel
    {
        public ClassModel() : base(DocumentKind.Class)
        {
        }
        
        public List<PropertyModel> Properties { get; set; }
        public List<MethodModel> Methods { get; set; }
        public List<MethodModel> Constructors { get; set; }
    }
}