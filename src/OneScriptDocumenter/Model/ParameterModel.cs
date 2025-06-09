/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Text.Json.Serialization;

namespace OneScriptDocumenter.Model
{
    public class ParameterModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string DefaultValue { get; set; }

        public bool IsOptional { get; set; }
    }
}