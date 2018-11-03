/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;

namespace ScriptEngine.Machine
{
    public struct CodeStatEntry
    {
        public readonly string ScriptFileName;
        public readonly string SubName;
        public readonly int LineNumber;

        public CodeStatEntry(string ScriptFileName, string SubName, int LineNumber)
        {
            this.ScriptFileName = ScriptFileName;
            this.SubName = SubName;
            this.LineNumber = LineNumber;
        }

        public override int GetHashCode()
        {
            return ScriptFileName.GetHashCode() + SubName.GetHashCode() + LineNumber;
        }

        public override bool Equals(object obj)
        {
            if (obj is CodeStatEntry)
            {
                var other = (CodeStatEntry)obj;
                return ScriptFileName.Equals(other.ScriptFileName, StringComparison.Ordinal)
                                     && SubName.Equals(other.SubName, StringComparison.Ordinal)
                                     && (LineNumber == other.LineNumber);
            }
            return false;
        }

        public override string ToString()
        {
            return $"{{{ScriptFileName}}}:{LineNumber}";
        }
    }
}
