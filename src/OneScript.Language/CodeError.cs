/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Text;

namespace OneScript.Language
{
    public struct CodeError
    {
        public string ErrorId { get; set; }
        
        public string Description { get; set; }
        
        public ErrorPositionInfo Position { get; set; }

        public override string ToString()
        {
            return
                $"[{ErrorId}] {Description} ({Position.LineNumber},{Position.ColumnNumber}) / {Position.ModuleName} / {Position.Code}";
        }

        public string ToString(ErrorDetails details)
        {
            if (details == ErrorDetails.Full)
                return ToString();

            var sb = new StringBuilder();
            if ((details & ErrorDetails.ErrorId) == ErrorDetails.ErrorId)
            {
                sb.Append('[').Append(ErrorId).Append("] ");
            }

            sb.Append(Description);
            
            if ((details & ErrorDetails.Position) == ErrorDetails.Position)
            {
                sb.Append(" (")
                    .Append(Position.LineNumber)
                    .Append(",")
                    .Append(Position.ColumnNumber)
                    .Append(")");
            }

            if ((details & ErrorDetails.ModuleName) == ErrorDetails.ModuleName)
            {
                sb.Append(" / ");
                sb.Append(Position.ModuleName);
            }
            
            if ((details & ErrorDetails.CodeFragment) == ErrorDetails.CodeFragment)
            {
                sb.Append(" / ");
                sb.Append(Position.Code);
            }

            return sb.ToString();
        }
        
        [Flags]
        public enum ErrorDetails
        {
            Full = 0,
            ErrorId = 1,
            Position = 2,
            ModuleName = 4,
            CodeFragment = 8,
            Short = Position | ModuleName,
            Simple = Position | ModuleName | CodeFragment
            
        }
    }
}