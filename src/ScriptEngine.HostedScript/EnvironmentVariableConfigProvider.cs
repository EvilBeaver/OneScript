/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Commons;
using ScriptEngine.Hosting;

namespace ScriptEngine.HostedScript
{
    public class EnvironmentVariableConfigProvider : IConfigProvider
    {
        private readonly string _variableName;

        public EnvironmentVariableConfigProvider(string variableName)
        {
            _variableName = variableName;
        }

        public string SourceId => _variableName;

        public IReadOnlyDictionary<string, string> Load()
        {
            var envValue = Environment.GetEnvironmentVariable(_variableName);
            if (string.IsNullOrEmpty(envValue))
                return new Dictionary<string, string>();

            var paramList = new FormatParametersList(envValue);
            return paramList.ToDictionary();
        }

        public string ResolveRelativePath(string path)
        {
            return path;
        }
    }
}
