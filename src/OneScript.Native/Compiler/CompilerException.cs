/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Commons;
using OneScript.Language;
using OneScript.Localization;

namespace OneScript.Native.Compiler
{
    public class CompilerException : BslCoreException
    {
        public ErrorPositionInfo Position { get; set; }

        public CompilerException(BilingualString message) : base(message)
        {
        }
        
        public CompilerException(BilingualString message, ErrorPositionInfo position) : base(message)
        {
            Position = position;
        }

        public override string Message => $"{base.Message} ({Position?.LineNumber ?? -1}\n{StackTrace}";
    }
}