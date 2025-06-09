/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OneScriptDocumenter.Model.Json
{
    public class SyntaxGroupJsonConverter : JsonConverter<SyntaxGroup>
    {
        public override SyntaxGroup Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, SyntaxGroup value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("title");
            writer.WriteStringValue(value.Title);

            if (value.Document != null)
            {
                writer.WritePropertyName("document");
                JsonSerializer.Serialize(writer, value.Document, value.Document.GetType(), options);
            }

            if (value.Items?.Count != 0)
            {
                writer.WritePropertyName("items");
                JsonSerializer.Serialize(writer, value.Items, options);
            }
            
            writer.WriteEndObject();
        }
    }
}