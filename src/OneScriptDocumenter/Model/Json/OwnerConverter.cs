/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using OneScriptDocumenter.Secondary;

namespace OneScriptDocumenter.Model.Json
{
    public class OwnerConverter : JsonConverter<Type>
    {
        public override Type Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
        {
            var linkId = ReferenceFactory.GetBslNameForAnnotatedObject(value);
            var encText = JsonEncodedText.Encode(linkId, options.Encoder);
            
            writer.WriteStringValue(encText);
        }
    }
}