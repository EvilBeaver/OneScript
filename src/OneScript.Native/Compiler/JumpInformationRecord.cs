/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Linq.Expressions;

namespace OneScript.Native.Compiler
{
    public struct JumpInformationRecord
    {
        public LabelTarget MethodReturn { get; set; }
        
        public LabelTarget LoopContinue { get; set; }
        
        public LabelTarget LoopBreak { get; set; }
    }
}