/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using OneScriptDocumenter.Model;

namespace OneScriptDocumenter.Cli
{
    public class JsonGenerator
    {
        private readonly DocumentationModel _documentation;

        public JsonGenerator(DocumentationModel documentation)
        {
            _documentation = documentation;
        }

        public void WriteFile(string outputFile)
        {
            ConsoleLogger.Info("Generating JSON output");
            
            using var file = new FileStream(outputFile, FileMode.Create);

            JsonSerializer.Serialize(file, _documentation, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                
                IgnoreReadOnlyProperties = false
            });
        }
    }
}