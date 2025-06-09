/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace OneScript.Contexts
{
    /// <summary>
    /// Атрибут обозначает свойство или метод, которое не является членом контекста
    /// однако, предоставляет xml-документацию для генерации синтакс-помощника.
    /// Используется для генерации перегрузок методов в документации совместно с флагом <see cref="ContextMethodAttribute.SkipForDocumenter"/> 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class DocumentedMemberAttribute : Attribute, INameAndAliasProvider
    {
        public DocumentedMemberAttribute(string name, string alias = null)
        {
            Name = name;
            Alias = alias;
        }

        public string Name { get; }
        public string Alias { get; }
    }
}