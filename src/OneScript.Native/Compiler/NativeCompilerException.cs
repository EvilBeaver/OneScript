/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Commons;
using OneScript.Language;
using OneScript.Localization;

namespace OneScript.Native.Compiler
{
    public class NativeCompilerException : BslCoreException
    {
        public ErrorPositionInfo Position { get; set; }

        public NativeCompilerException(BilingualString message) : base(message)
        {
        }
        
        public NativeCompilerException(BilingualString message, ErrorPositionInfo position) : base(message)
        {
            Position = position;
        }
        
        public NativeCompilerException(BilingualString message, ErrorPositionInfo position, Exception innerException) : base(message, innerException)
        {
            Position = position;
        }
    }
}