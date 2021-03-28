/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Linq.Expressions;
using FluentAssertions;
using OneScript.StandardLibrary.Json;
using OneScript.StandardLibrary.Native;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Values;
using ScriptEngine.Types;
using Xunit;

namespace OneScript.Core.Tests
{
    public class NativeCompilerTest
    {
        [Fact]
        public void CanInjectContext_As_Symbols()
        {
            var context = new GlobalJsonFunctions();
            var scope = SymbolScope.FromContext(context);

            scope.Methods.Should().HaveCount(3);
            scope.Methods.IndexOf("ЗаписатьJSON").Should().BeGreaterOrEqualTo(0);
        }

        [Fact]
        public void Can_Compile_Assignment()
        {
            var blockOfCode = new CompiledBlock(new DefaultTypeManager());
            
            blockOfCode.Parameters.Insert("MyVar", new TypeTypeValue(BasicTypes.Number));
            blockOfCode.CodeBlock = "MyVar = 2";

            var expr = blockOfCode.MakeExpression();
            expr.Body.As<BlockExpression>().Expressions.Should().HaveCount(1);
            expr.Body.As<BlockExpression>().Expressions[0].Should().BeAssignableTo<BinaryExpression>();

            expr.Parameters.Should().HaveCount(1);
        }

        [Fact]
        public void Can_Compile_Binary_Expressions()
        {
            var blockOfCode = new CompiledBlock(new DefaultTypeManager());
            
            blockOfCode.Parameters.Insert("MyVar", new TypeTypeValue(BasicTypes.Number));
            blockOfCode.CodeBlock = "MyVar = MyVar + 1";

            var expr = blockOfCode.MakeExpression();

            var body = expr.Body.As<BlockExpression>().Expressions;
            
            body[0].As<BinaryExpression>().NodeType.Should().Be(ExpressionType.Assign);
            body[0].As<BinaryExpression>().Right.Should().BeAssignableTo<BinaryExpression>();
        }
        
    }
}