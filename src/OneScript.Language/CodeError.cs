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
            /// <summary>
            /// Только текст сообщения
            /// </summary>
            MessageOnly = 0,
            
            /// <summary>
            /// Показывать идентификатор ошибки
            /// </summary>
            ErrorId = 1,
            
            /// <summary>
            /// Показывать позицию (Строка,Колонка)
            /// </summary>
            Position = 2,
            
            /// <summary>
            /// Показывать имя модуля с ошибкой
            /// </summary>
            ModuleName = 4,
            
            /// <summary>
            /// Показывать фрагмент кода с ошибкой
            /// </summary>
            CodeFragment = 8,
            
            /// <summary>
            /// Показывать позицию и имя модуля
            /// </summary>
            Short = Position | ModuleName,
            
            /// <summary>
            /// Показывать позицию, имя модуля, фрагмент кода
            /// </summary>
            Simple = Position | ModuleName | CodeFragment,
                
            /// <summary>
            /// Вся информация об ошибке
            /// </summary>
            Full = 16
        }
    }
}