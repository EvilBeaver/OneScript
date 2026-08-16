/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using OneScript.Values;

namespace OneScript.Native.Compiler
{
    internal static class DateOperations
    {
        /// <summary>
        ///  Операция смещения дат (прибавление или удаление секунд
        /// </summary>
        /// <param name="left">Выражение с типом DateTime</param>
        /// <param name="right">Выражение с типом Число</param>
        /// <param name="opCode">Вид операции Add или Substract</param>
        /// <returns>Выражение содержащее операцию</returns>
        /// <exception cref="NativeCompilerException">opCode указан неверно</exception>
        public static Expression DateOffsetOperation(Expression left, Expression right, ExpressionType opCode)
        {
            Debug.Assert(left.Type == typeof(DateTime));
            Debug.Assert(right.Type == typeof(decimal));
            
            var method = opCode == ExpressionType.Add
                ? typeof(BslDateValue).GetMethod(nameof(BslDateValue.AddSeconds), new[] { typeof(DateTime), typeof(decimal) })
                : typeof(BslDateValue).GetMethod(nameof(BslDateValue.SubtractSeconds), new[] { typeof(DateTime), typeof(decimal) });
            Debug.Assert(method != null);

            return Expression.Call(method, left, right);
        }
        
        /// <summary>
        /// Операция разности дат (вычитания дат)
        /// </summary>
        /// <param name="left">Выражение с типом DateTime</param>
        /// <param name="right">Выражение с типом DateTime</param>
        /// <returns>Выражение, содержащее операцию</returns>
        public static Expression DateDiffExpression(Expression left, Expression right)
        {
            Debug.Assert(left.Type == typeof(DateTime));
            Debug.Assert(right.Type == typeof(DateTime));
            
            var spanExpr = Expression.Subtract(left, right);
            var totalSeconds = Expression.Property(spanExpr, nameof(TimeSpan.TotalSeconds));
            var decimalSeconds = Expression.Convert(totalSeconds, typeof(decimal));

            return decimalSeconds;
        }
        
        /// <summary>
        /// Операция сравнения двух дат
        /// </summary>
        /// <param name="left">Выражение с типом DateTime</param>
        /// <param name="right">Выражение с типом DateTime</param>
        /// <param name="opCode">Двоичный оператор</param>
        /// <returns>Выражение, содержащее операцию</returns>
        /// <exception cref="NativeCompilerException">opCode указан неверно</exception>
        public static Expression DatesBinaryOperation(Expression left, Expression right, ExpressionType opCode)
        {
            Debug.Assert(left.Type == typeof(DateTime));
            Debug.Assert(right.Type == typeof(DateTime));
            
            if (IsComparisonOperation(opCode) || IsEqualityOperation(opCode))
            {
                return Expression.MakeBinary(opCode, left, right);
            }

            if (opCode != ExpressionType.Subtract)
                throw NativeCompilerException.OperationNotDefined(opCode, left.Type, right.Type);
                
            return DateDiffExpression(left, right);
        }
        
        private static bool IsComparisonOperation(ExpressionType opCode)
        {
            return opCode == ExpressionType.LessThan ||
                   opCode == ExpressionType.LessThanOrEqual ||
                   opCode == ExpressionType.GreaterThan ||
                   opCode == ExpressionType.GreaterThanOrEqual;
        }
        
        private static bool IsEqualityOperation(ExpressionType opCode)
        {
            return opCode == ExpressionType.Equal || opCode == ExpressionType.NotEqual;
        }
    }
}