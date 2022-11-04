/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Native.Runtime;
using OneScript.Values;

namespace OneScript.Native.Compiler
{
    internal class BinaryOperationCompiler
    {
        private ExpressionType _opCode;

        public Expression Compile(BinaryOperationNode node, Expression left, Expression right)
        {
            _opCode = ExpressionHelpers.TokenToOperationCode(node.Operation);
            
            if (left.Type.IsValue() || right.Type.IsValue())
            {
                return CompileDynamicOperation(left, right);
            }
            else
            {
                return CompileStaticOperation(left, right);
            }
        }

        private Expression CompileStaticOperation(Expression left, Expression right)
        {
            if (IsEqualityOperation(_opCode))
            {
                return MakeStaticEqualityOperation(left, right);
            }
            
            if (left.Type.IsNumeric())
            {
                return MakeNumericOperation(left, right);
            }
            
            if (left.Type == typeof(DateTime))
            {
                return DateOperation(left, right);
            }
            
            if (left.Type == typeof(string))
            {
                if (_opCode == ExpressionType.Add)
                    return StringAddition(left, right);

                // Для строк допустимо сравнение со строками на < >
                if (IsComparisonOperation(_opCode))
                {
                    // для простоты сделаем через BslValue.CompareTo
                    return MakeDynamicComparison(left, right);
                }
            }
            
            if (left.Type == typeof(bool))
            {
                return MakeLogicalOperation(left, right);
            }
            
            throw NativeCompilerException.OperationNotDefined(_opCode, left.Type, right.Type);
        }

        private Expression MakeStaticEqualityOperation(Expression left, Expression right)
        {
            return Expression.MakeBinary(_opCode, left, right);
        }

        private Expression MakeNumericOperation(Expression left, Expression right)
        {
            Debug.Assert(left.Type.IsNumeric());
            
            if (right.Type.IsNumeric())
            {
                return AdjustArithmeticOperandTypesAndMakeBinary(left, right);
            }
            
            throw NativeCompilerException.OperationNotDefined(_opCode, left.Type, right.Type);
        }

        private Expression AdjustArithmeticOperandTypesAndMakeBinary(Expression left, Expression right)
        {
            if (left.Type == right.Type)
                return Expression.MakeBinary(_opCode, left, right);
            
            // try find direct operator
            var method = GetUserDefinedBinaryOperator("op_" + _opCode.ToString(), left.Type, right.Type);
            if (method == null)
            {
                // try convert
                if (left.Type.IsInteger() && !right.Type.IsInteger())
                {
                    // нужна нецелочисленная операция
                    left = Expression.Convert(left, typeof(decimal));
                    return AdjustArithmeticOperandTypesAndMakeBinary(left, right);
                }

                right = Expression.Convert(right, left.Type);
            }

            return Expression.MakeBinary(_opCode, left, right, false, method);
        }
        
        private static MethodInfo GetUserDefinedBinaryOperator(string name, Type leftType, Type rightType) {
 
            Type[] types = { leftType, rightType };
            
            BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            MethodInfo method = leftType.GetMethod(name, flags, null, types, null);
            if (method == null)
            {
                method = rightType.GetMethod(name, flags, null, types, null);
            }
            
            return method;
        }
        
        private Expression DateOperation(Expression left, Expression right)
        {
            if (right.Type.IsNumeric())
            {
                return DateOperations.DateOffsetOperation(left, right, _opCode);
            }

            if (right.Type == typeof(DateTime))
            {
                return DateOperations.DatesBinaryOperation(left, right, _opCode);
            }

            throw NativeCompilerException.OperationNotDefined(_opCode, left.Type, right.Type);
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

        private Expression MakeLogicalOperation(Expression left, Expression right)
        {
            return Expression.MakeBinary(_opCode, ExpressionHelpers.ToBoolean(left), ExpressionHelpers.ToBoolean(right));
        }
        
        private Expression StringAddition(Expression left, Expression right)
        {
            var concatMethod = typeof(string).GetMethod(
                nameof(string.Concat),
                new[] { typeof(string), typeof(string) });

            Debug.Assert(concatMethod != null);
            
            return Expression.Call(null, concatMethod, left, ExpressionHelpers.ToString(right));
        }
        
        private Expression CompileDynamicOperation(Expression left, Expression right)
        {
            switch (_opCode)
            {
                case ExpressionType.Add:
                    return ExpressionHelpers.DynamicAdd(left, right);
                case ExpressionType.Subtract:
                    return ExpressionHelpers.DynamicSubtract(left, right);
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                    return MakeDynamicComparison(left, right); 
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    return MakeDynamicEquality(left, right);
                case ExpressionType.Multiply:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                    return MakeNumericOperation(
                        ExpressionHelpers.ToNumber(left),
                        ExpressionHelpers.ToNumber(right));
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    return MakeLogicalOperation(left, right);
                default:
                    throw new NativeCompilerException($"Operation {_opCode} is not defined for IValues");
            }
        }

        private Expression MakeDynamicEquality(Expression left, Expression right)
        {
            var result = ExpressionHelpers.CallEquals(ExpressionHelpers.ConvertToBslValue(left), ExpressionHelpers.ConvertToBslValue(right));
            if (_opCode == ExpressionType.NotEqual)
                result = Expression.Not(result);

            return result;
        }

        private Expression MakeDynamicComparison(Expression left, Expression right)
        {
            var compareCall = ExpressionHelpers.CallCompareTo(ExpressionHelpers.ConvertToBslValue(left), ExpressionHelpers.ConvertToBslValue(right));
            return Expression.MakeBinary(_opCode, compareCall, Expression.Constant(0));
        }
    }
}