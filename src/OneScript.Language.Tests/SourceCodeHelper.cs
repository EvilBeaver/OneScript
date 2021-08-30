/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Sources;

namespace OneScript.Language.Tests
{
    public static class SourceCodeHelper
    {
        public static SourceCode FromString(string code)
        {
            return SourceCodeBuilder.Create()
                .FromSource(new StringSource(code))
                .Build();
        }

        private class StringSource : ICodeSource
        {
            private readonly string _code;

            public StringSource(string code)
            {
                _code = code;
            }

            public string Location => _code;
            public string GetSourceCode() => _code;
        }
    }
}