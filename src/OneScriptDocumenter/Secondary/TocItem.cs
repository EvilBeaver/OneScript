/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OneScriptDocumenter.Secondary
{
    public class TocItem
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("items")]
        public List<TocItem> Items { get; set; }
        
        [JsonPropertyName("page")] 
        public string GeneratePage { get; set; }
    }
}