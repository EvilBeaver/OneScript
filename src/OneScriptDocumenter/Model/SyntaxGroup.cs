/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Text.Json.Serialization;
using OneScriptDocumenter.Model.Json;

namespace OneScriptDocumenter.Model
{
    [JsonConverter(typeof(SyntaxGroupJsonConverter))]
    public class SyntaxGroup
    {
        private string _selfTitle;
        
        public string Title
        {
            get => _selfTitle ?? Document?.Title;
            set => _selfTitle = value;
        }
        
        public IDocument Document { get; set; }
        public IList<SyntaxGroup> Items { get; set; } = new List<SyntaxGroup>();

        public string Page { get; set; }
    }
}