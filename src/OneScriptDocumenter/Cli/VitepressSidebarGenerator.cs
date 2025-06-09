/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using OneScriptDocumenter.Model;
using OneScriptDocumenter.Secondary;

namespace OneScriptDocumenter.Cli
{
    public class VitepressSidebarGenerator : BaseModelVisitor
    {
        private readonly string _outputFile;
        private readonly ReferenceFactory _referenceFactory;
        
        private int _level = 0;

        private Utf8JsonWriter _writer;
        private const string othersGroupName = "Прочее";

        public VitepressSidebarGenerator(string outputFile, ReferenceFactory referenceFactory)
        {
            _outputFile = outputFile;
            _referenceFactory = referenceFactory;
        }

        public override void VisitModel(DocumentationModel model)
        {
            using var jsonOutput = new FileStream(_outputFile, FileMode.Create);
            _writer = new Utf8JsonWriter(jsonOutput, new JsonWriterOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                Indented = true
            });
            
            _writer.WriteStartArray();
            base.VisitModel(model);
            _writer.WriteEndArray();
            _writer.Dispose();
        }

        protected override void VisitSyntaxGroup(SyntaxGroup model)
        {
            _writer.WriteStartObject();
            
            _writer.WriteString("text", model.Title);

            var link = GenerateLink(model);
            if (link != null)
            {
                _writer.WriteString("link", link);
            }

            var hasChildren = model?.Items.Count > 0 && model.Title != othersGroupName; 
            if (_level >= 1 && hasChildren)
            {
                _writer.WriteBoolean("collapsed", true);
            }

            if (hasChildren)
            {
                _level++;
                _writer.WritePropertyName("items");
                _writer.WriteStartArray();
                base.VisitChildItems(model);
                _writer.WriteEndArray();
                _level--;
            }
            
            _writer.WriteEndObject();
        }

        private string GenerateLink(SyntaxGroup model)
        {
            if (model.Page != null)
            {
                return _referenceFactory.BaseUrl + model.Page;
            }
            else if (model.Document != null)
            {
                return _referenceFactory.CreateReference(model.Document.Owner);
            }
            
            return null;
        }
    }
}