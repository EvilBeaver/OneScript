/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScriptEngine.Hosting
{
    public class ConfigurationValue
    {
        public string RawValue { get; }
        public IConfigProvider Source { get; }

        public ConfigurationValue(string rawValue, IConfigProvider source)
        {
            RawValue = rawValue;
            Source = source;
        }

        /// <summary>
        /// Разрешает значение как путь относительно источника конфигурации
        /// </summary>
        public string ResolvePath()
        {
            return string.IsNullOrEmpty(RawValue) ? RawValue : Source.ResolveRelativePath(RawValue.Trim());
        }

        /// <summary>
        /// Разрешает значение как список путей
        /// </summary>
        public IEnumerable<string> ResolvePathList(char separator = ';')
        {
            if (string.IsNullOrEmpty(RawValue))
                return Enumerable.Empty<string>();

            return RawValue.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries)
                .Select(path =>
                {
                    var trimmed = path.Trim();
                    return Source.ResolveRelativePath(trimmed);
                });
        }
    }
}
