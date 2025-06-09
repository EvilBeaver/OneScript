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
    /// Атрибут обозначает класс, который сам по себе не является типом или контекстом BSL,
    /// однако имеет методы, которые предоставляют xml-документацию для генерации синтакс-помощника
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DocumentationProviderAttribute : Attribute
    {
        public DocumentationProviderAttribute(string category)
        {
            Category = category;
        }
        
        public string Category { get; }
    }
}