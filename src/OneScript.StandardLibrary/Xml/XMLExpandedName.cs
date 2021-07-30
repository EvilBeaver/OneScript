/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Xml;
using OneScript.Contexts;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Xml
{
    /// <summary>
    /// Представляет полное имя XML.
    /// </summary>
    /// <see cref="XmlQualifiedName"/>
    [ContextClass("РасширенноеИмяXML", "XMLExpandedName")]
    public class XMLExpandedName : AutoContext<XMLExpandedName>
    {
        public XMLExpandedName(string namespaceURI, string localName)
            => NativeValue = new XmlQualifiedName(localName, namespaceURI);

        public XMLExpandedName(XmlQualifiedName qualifiedName) => NativeValue = qualifiedName;

        public XmlQualifiedName NativeValue { get; }

        public override string ToString() => NativeValue.ToString();

        /// <summary>
        /// URI пространства имен.
        /// </summary>
        [ContextProperty("URIПространстваИмен", "NamespaceURI")]
        public string NamespaceURI => NativeValue.Namespace;

        /// <summary>
        /// Локальное имя в пространстве имен.
        /// </summary>
        [ContextProperty("ЛокальноеИмя", "LocalName")]
        public string LocalName => NativeValue.Name;

        /// <summary>
        /// По локальному имени и URI пространства имен.
        /// </summary>
        /// <param name="namespaceURI">URI пространства имен.</param>
        /// <param name="localName">Локальное имя.</param>
        /// <returns>Новое полное имя XML.</returns>
        [ScriptConstructor(Name = "По умолчанию")]
        public static XMLExpandedName Create(IValue namespaceURI, IValue localName)
            => new XMLExpandedName(namespaceURI.AsString(), localName.AsString());

        public override bool Equals(IValue other)
        {
            if (other.AsObject() is XMLExpandedName _expandedName)
                return NativeValue.Equals(_expandedName.NativeValue);
            else
                return base.Equals(other);
        }

        protected override string ConvertToString() => $"{{{NamespaceURI}}}{LocalName}";
    }
}