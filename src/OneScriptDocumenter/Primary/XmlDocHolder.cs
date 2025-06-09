/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace OneScriptDocumenter.Primary
{
    public class XmlDocHolder
    {
        private readonly string _libraryName;
        private readonly Dictionary<string, XElement> _memberDocumentation = new Dictionary<string, XElement>();

        public XmlDocHolder(string libraryName)
        {
            _libraryName = libraryName;
        }
        
        public void Read(string docPath)
        {
            using var reader = new StreamReader(docPath);
            var xmlDoc = XDocument.Load(reader);
                
            var asmElement = xmlDoc.Root?.Element("assembly");
            if (asmElement == null)
                throw new ArgumentException("Wrong XML doc format");
                
            var fileLibName = asmElement.Element("name")?.Value;
            if (string.Compare(_libraryName, fileLibName, true, System.Globalization.CultureInfo.InvariantCulture) != 0)
                throw new ArgumentNullException(
                    $"Mismatch assembly names. Expected {_libraryName}, found in XML {fileLibName}");

            var members = xmlDoc.Element("doc")?.Element("members")?.Elements() ?? Array.Empty<XElement>();
            _memberDocumentation.Clear();
            foreach (var item in members)
            {
                var key = item.Attribute("name")?.Value ?? throw new ArgumentException($"Member {item} has no attribute \"name\"");
                _memberDocumentation[key] = item;
            }
        }

        public XElement this[string key]
        {
            get
            {
                var hasKey = _memberDocumentation.TryGetValue(key, out var result);
                return hasKey ? result : null;
            }
        }
    }
}