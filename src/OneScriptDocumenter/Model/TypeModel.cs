/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using OneScript.Localization;
using OneScriptDocumenter.Model.Json;

namespace OneScriptDocumenter.Model
{
    public abstract class TypeModel : IDocument
    {
        protected TypeModel(DocumentKind kind)
        {
            Kind = kind;
        }

        public DocumentKind Kind { get; }
        public BilingualString Name { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Description { get; set; }

        public string Title => Name.Russian;
        
        [JsonConverter(typeof(OwnerConverter))]
        public Type Owner { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Example { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<string> SeeAlso { get; set; }
    }
}