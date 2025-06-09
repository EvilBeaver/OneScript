/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Localization;

namespace OneScriptDocumenter.Model
{
    public class EnumModel : TypeModel
    {
        public EnumModel() : base(DocumentKind.Enum)
        {
        }

        public List<EnumItemModel> Items { get; set; } = new List<EnumItemModel>();
    }
    
    public class EnumItemModel
    {
        public BilingualString Name { get; set; }
        public string Description { get; set; }
    }
}