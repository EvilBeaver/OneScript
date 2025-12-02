/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

namespace OneScriptDocumenter.Primary
{
    public class PrimaryBslDocument
    {
        public Type Owner { get; set; }

        public OwnerKind OwnerKind { get; set; }

        public string XmlDocIdentifier { get; set; } = "";
        
        public string Title { get; set; } = "";
        
        public XElement? SelfDoc { get; set; }

        public Dictionary<PropertyInfo, XElement> Properties { get; } = new Dictionary<PropertyInfo, XElement>();
        public Dictionary<MethodInfo, XElement> Methods { get; } = new Dictionary<MethodInfo, XElement>();
        public Dictionary<FieldInfo, XElement> Fields { get; } = new Dictionary<FieldInfo, XElement>();
        public Dictionary<MethodInfo, XElement> Constructors { get; } = new Dictionary<MethodInfo, XElement>();
    }
}