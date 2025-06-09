/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Text.Json.Serialization;
using OneScript.Localization;

namespace OneScriptDocumenter.Model
{
    public class PropertyModel
    {
        public BilingualString Name { get; set; }

        public string ClrName { get; set; }
        
        public string Title => Name.Russian;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Description { get; set; } = "";

        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Returns { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Example { get; set; } = "";
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<string> SeeAlso { get; set; }
    }
}