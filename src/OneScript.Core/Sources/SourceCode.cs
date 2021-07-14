/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace OneScript.Sources
{
    public class SourceCode
    {
        private readonly ICodeSource _textSource;

        public SourceCode(ICodeSource textSource)
        {
            _textSource = textSource;
        }

        public string ModuleName => _textSource.ModuleName;

        public string Location => _textSource.Location;

        public string GetSourceCode() => _textSource.GetSourceCode();
        
        public string GetCodeLine(int line)
        {
            throw new NotImplementedException();
        }
    }
}