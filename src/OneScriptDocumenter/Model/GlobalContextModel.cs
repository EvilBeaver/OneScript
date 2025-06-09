/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using OneScriptDocumenter.Model.Json;

namespace OneScriptDocumenter.Model
{
    public class GlobalContextModel : IDocument
    {
        public GlobalContextModel()
        {
            Kind = DocumentKind.GlobalContext;
        }

        public DocumentKind Kind { get; }
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        public List<PropertyModel> Properties { get; set; }
        
        public List<MethodModel> Methods { get; set; }

        [JsonConverter(typeof(OwnerConverter))]
        public Type Owner { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<string> SeeAlso { get; set; }
    }
}