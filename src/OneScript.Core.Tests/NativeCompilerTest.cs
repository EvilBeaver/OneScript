/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using OneScript.DependencyInjection;
using OneScript.Native.Compiler;
using OneScript.Native.Runtime;
using OneScript.Runtime.Binding;
using OneScript.StandardLibrary;
using OneScript.StandardLibrary.Collections;
using OneScript.StandardLibrary.Collections.ValueList;
using OneScript.StandardLibrary.Collections.ValueTable;
using OneScript.StandardLibrary.Json;
using OneScript.StandardLibrary.Native;
using OneScript.StandardLibrary.Text;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;
using ScriptEngine.Types;
using Xunit;

namespace OneScript.Core.Tests
{
    public class NativeCompilerTest
    {
        private CompiledBlock GetCompiler(Action<ITypeManager, IServiceDefinitions> setup)
        {
            var tm = new DefaultTypeManager();
            var services = new TinyIocImplementation();
            services.Register<ITypeManager>(tm);
            setup(tm, services);

            return new CompiledBlock(services.CreateContainer());
        }
        
        private CompiledBlock GetCompiler(ITypeManager tm)
        {
            var services = new TinyIocImplementation();
            services.Register(tm);

            return new CompiledBlock(services.CreateContainer());
        }
        
        [Fact]
        public void CanInjectContext_As_Symbols()
        {
            var context = new GlobalJsonFunctions();
            var scope = SymbolScope.FromObject(context);

            scope.Methods.Should().HaveCount(3);
            scope.Methods.IndexOf("ЗаписатьJSON").Should().BeGreaterOrEqualTo(0);
        }

        [Fact]
        public void Can_Compile_Assignment()
        {
            var blockOfCode = new CompiledBlock(default);
            
            blockOfCode.Parameters.Insert("MyVar", new BslTypeValue(BasicTypes.Number));
            blockOfCode.CodeBlock = "MyVar = 2";

            var expr = blockOfCode.MakeExpression();
            expr.Body.As<BlockExpression>().Expressions.Should().HaveCount(2); // в конце всегда неявный return
            expr.Body.As<BlockExpression>().Expressions[0].Should().BeAssignableTo<BinaryExpression>();

            expr.Parameters.Should().HaveCount(1);
        }

        [Fact]
        public void Can_Compile_Binary_Expressions()
        {
            var blockOfCode = new CompiledBlock(default);
            
            blockOfCode.Parameters.Insert("MyVar", new BslTypeValue(BasicTypes.Number));
            blockOfCode.CodeBlock = "MyVar = MyVar + 1";

            var expr = blockOfCode.MakeExpression();

            var body = expr.Body.As<BlockExpression>().Expressions;
            
            body[0].As<BinaryExpression>().NodeType.Should().Be(ExpressionType.Assign);
            body[0].As<BinaryExpression>().Right.Should().BeAssignableTo<BinaryExpression>();
        }
        
        [Fact]
        public void Can_Compile_Unary_Expressions()
        {
            var blockOfCode = new CompiledBlock(default);
            
            blockOfCode.Parameters.Insert("MyVar", new BslTypeValue(BasicTypes.Number));
            blockOfCode.CodeBlock = "MyVar = -MyVar";

            var expr = blockOfCode.MakeExpression();

            var body = expr.Body.As<BlockExpression>().Expressions;
            
            body[0].As<BinaryExpression>().NodeType.Should().Be(ExpressionType.Assign);
            body[0].As<BinaryExpression>().Right.Should().BeAssignableTo<UnaryExpression>();
        }

        [Fact]
        public void Can_Compile_Empty_Body()
        {
            var blockOfCode = new CompiledBlock(default);
            var func = blockOfCode.CreateDelegate();
            var result = func(default);
            Assert.Equal(BslUndefinedValue.Instance, result);
        }
        
        [Fact]
        public void Can_Compile_AcceptParameters_In_Array()
        {
            var blockOfCode = new CompiledBlock(default);
            blockOfCode.Parameters.Insert("A", new BslTypeValue(BasicTypes.Number));
            blockOfCode.Parameters.Insert("Б", new BslTypeValue(BasicTypes.String));
            
            var func = blockOfCode.CreateDelegate();
            var result = func(new BslValue[]{ BslNumericValue.Create(1), BslStringValue.Create("hello") });
            
            Assert.Equal(BslUndefinedValue.Instance, result);
        }
        
        [Fact]
        public void Can_Compile_AcceptParameters()
        {
            var blockOfCode = new CompiledBlock(default);
            blockOfCode.Parameters.Insert("A", new BslTypeValue(BasicTypes.Number));
            blockOfCode.Parameters.Insert("Б", new BslTypeValue(BasicTypes.String));
            
            var func = blockOfCode.CreateDelegate<Func<decimal, string, BslValue>>();
            var result = func(1, "привет");
            
            Assert.Equal(BslUndefinedValue.Instance, result);
        }

        [Fact]
        public void Number_To_Number_Operations_Are_Available()
        {
            var block = new CompiledBlock(default);
            block.CodeBlock = "А = 1+1*2/3%4";
            var assignment = block.MakeExpression()
                .Body
                .As<BlockExpression>()
                .Expressions
                .First();

            assignment.NodeType.Should().Be(ExpressionType.Assign);
        }
        
        [Fact]
        public void Number_To_IValue_Operations_Are_Available()
        {
            var block = new CompiledBlock(default);
            block.CodeBlock = "А = 1+(А*Б/3)%В";
            
            block.Parameters.Insert("А", new BslTypeValue(BasicTypes.Number));
            block.Parameters.Insert("Б", new BslTypeValue(BasicTypes.Number));
            block.Parameters.Insert("В", new BslTypeValue(BasicTypes.Number));
            
            var assignment = block.MakeExpression()
                .Body
                .As<BlockExpression>()
                .Expressions
                .First();
            
            assignment.NodeType.Should().Be(ExpressionType.Assign);

        }
        
        [Fact]
        public void DateAddition_Is_Availiable()
        {
            var block = new CompiledBlock(default);
            block.CodeBlock = "А = '19840331'+(86400 * 37)";
            
            var assignment = block.MakeExpression()
                .Body
                .As<BlockExpression>()
                .Expressions
                .First();
            
            assignment.NodeType.Should().Be(ExpressionType.Assign);
            var expr = assignment.As<BinaryExpression>();

            expr.Left.Type.Should().Be<DateTime>();
            expr.Right.Type.Should().Be<DateTime>();
        }
        
        [Fact]
        public void DateDiff_Available()
        {
            var block = new CompiledBlock(default);
            block.CodeBlock = "F = (Сегодня - '19840331') / 86400 / 366";
            block.Parameters.Insert("Сегодня", new BslTypeValue(BasicTypes.Date));
            var assignment = block.MakeExpression()
                .Body
                .As<BlockExpression>()
                .Expressions
                .First();
            
            assignment.NodeType.Should().Be(ExpressionType.Assign);
            var expr = assignment.As<BinaryExpression>();

            expr.Left.Type.Should().Be<decimal>();
            expr.Right.Type.Should().Be<decimal>();
        }
        
        [Fact]
        public void Parameter_Passing_And_Return_Is_Available()
        {
            var block = new CompiledBlock(default);
            block.CodeBlock = "Возврат (Сегодня - '19840331') / 86400 / 366";
            block.Parameters.Insert("Сегодня", new BslTypeValue(BasicTypes.Date));

            var beaverAge = block.CreateDelegate<Func<DateTime, BslValue>>();
            var age = (decimal)(BslNumericValue)beaverAge(DateTime.Now);

            age.Should().BeGreaterThan(0);
        }
        
        [Fact]
        public void Can_Assign_To_Indexer()
        {
            var tm = new DefaultTypeManager();
            var arrayType = tm.RegisterClass(typeof(ArrayImpl));
            
            var block = new CompiledBlock(default);
            block.CodeBlock = "Arr[5] = 15";
            block.Parameters.Insert("Arr", new BslTypeValue(arrayType));

            var expr = block.MakeExpression();
            var statement = expr.Body.As<BlockExpression>().Expressions.First();

            if (statement.NodeType == ExpressionType.Assign)
            {
                var assign = statement.As<BinaryExpression>();
                assign.Left.NodeType.Should().Be(ExpressionType.Index);
            }
            else if (statement.NodeType != ExpressionType.Dynamic)
            {
                statement.NodeType.Should().Be(ExpressionType.Call);
                statement.As<MethodCallExpression>().Method.Name.Should().Be("BslIndexSetter");
            }

            var proc = expr.Compile();
            var array = new ArrayImpl(new IValue[6]);
            proc.DynamicInvoke(array);

            array.Get(5).AsNumber().Should().Be(15M);

        }
        
        [Fact(Skip = "Будет переделываться на индексатор this")]
        public void Can_Read_Special_StaticIndexer()
        {
            var tm = new DefaultTypeManager();
            var arrayType = tm.RegisterClass(typeof(ArrayImpl));
            
            var block = new CompiledBlock(default);
            block.CodeBlock = "А = Arr[5]; Возврат А;";
            block.Parameters.Insert("Arr", new BslTypeValue(arrayType));

            var expr = block.MakeExpression();
            var statement = expr.Body.As<BlockExpression>().Expressions.First();

            statement.NodeType.Should().Be(ExpressionType.Assign);
            var assign = statement.As<BinaryExpression>();
            
            assign.Right.NodeType.Should().Be(ExpressionType.Call);

            var arr = new ArrayImpl(new IValue[6]);
            arr.Set(5, ValueFactory.Create("Hello"));

            var func = expr.Compile();
            var result = (BslValue)func.DynamicInvoke(arr);
            result.ToString().Should().Be("Hello");
        }

        [Fact]
        public void Can_Do_While()
        {
            var block = new CompiledBlock(default);
            block.Parameters.Insert("Результат", new BslTypeValue(BasicTypes.Number));
            block.CodeBlock = "Ф = 1;" +
                              "Пока Ф < 10 Цикл" +
                              "\tРезультат = Результат + Ф;" +
                              "\tФ = Ф + 1;" +
                              "\tЕсли Ф > 2 Тогда Прервать; КонецЕсли;" +
                              "КонецЦикла;" +
                              "Возврат Результат;";
            var func = block.MakeExpression().Compile();
            
            var args = new object[] { decimal.One };
            var result = (decimal)(BslNumericValue)func.DynamicInvoke(args);
            result.Should().Be(4);
        }
        
        [Fact]
        public void Can_Do_IfThen()
        {
            var block = new CompiledBlock(default);
            block.CodeBlock = "Если Истина Тогда Ф=1; КонецЕсли";
            var loop = block.MakeExpression()
                .Body
                .As<BlockExpression>()
                .Expressions
                .First();
            loop.NodeType.Should().Be(ExpressionType.Conditional);
        }
        
        [Fact]
        public void Can_Do_IfThenElse()
        {
            var block = new CompiledBlock(default);
            block.CodeBlock = "Если Истина Тогда Ф=1; Иначе Ф=2; КонецЕсли";
            var loop = block.MakeExpression()
                .Body
                .As<BlockExpression>()
                .Expressions
                .First();
            loop.NodeType.Should().Be(ExpressionType.Conditional);
        }
        
        [Fact]
        public void Can_Do_ElseIfElse()
        {
            var block = new CompiledBlock(default);
            block.Parameters.Insert("П", new BslTypeValue(BasicTypes.Number));
            block.Parameters.Insert("Ф", new BslTypeValue(BasicTypes.Number));
            block.CodeBlock = 
                "Если П=1 Тогда Ф=1;" +
                "ИначеЕсли П=2 Тогда Ф=2;" +
                "ИначеЕсли П=3 Тогда Ф=3;" +
                "Иначе Ф=0; КонецЕсли;" +
                "Возврат Ф;";
            var expression = block.MakeExpression(); 
            var condition = expression 
                .Body
                .As<BlockExpression>()
                .Expressions
                .First();
            condition.NodeType.Should().Be(ExpressionType.Conditional);
            var func = expression.Compile();

            for (decimal i = 0; i < 4; i++)
            {
                var args = new object[] {i, (decimal)0};
                var result = (BslNumericValue)func.DynamicInvoke(args);
                ((decimal)result).Should().Be(i);
            }
        }
        
        [Fact]
        public void Can_Do_ElseIf_WithoutElse()
        {
            var block = new CompiledBlock(default);
            block.Parameters.Insert("П", new BslTypeValue(BasicTypes.Number));
            block.Parameters.Insert("Ф", new BslTypeValue(BasicTypes.Number));
            block.CodeBlock = 
                "Если П=1 Тогда Ф=1;" +
                "ИначеЕсли П=2 Тогда Ф=2;" +
                "ИначеЕсли П=3 Тогда Ф=3;" +
                "ИначеЕсли П=4 Тогда Ф=4; КонецЕсли;" +
                "Возврат Ф;";
            var expression = block.MakeExpression(); 
            var condition = expression 
                .Body
                .As<BlockExpression>()
                .Expressions
                .First();
            condition.NodeType.Should().Be(ExpressionType.Conditional);
            var func = expression.Compile();

            for (decimal i = 0; i < 4; i++)
            {
                var args = new object[] {i, 0M};
                var result = (BslNumericValue)func.DynamicInvoke(args);
                ((decimal)result).Should().Be(i);
            }
        }
        
        [Fact]
        public void Can_ForLoop()
        {
            var block = new CompiledBlock(default);
            block.Parameters.Insert("Результат", new BslTypeValue(BasicTypes.Number));
            block.CodeBlock = 
                "Для Ф = 1 По 2+2*2 Цикл " +
                "Результат = Результат + Ф;" +
                "Если Ф > 2 Тогда Прервать; КонецЕсли; " +
                "Продолжить;" +
                "КонецЦикла;" +
                "Возврат Результат;";
            var expression = block.MakeExpression();
            var func = expression.Compile();
            var args = new object[] { decimal.Zero };
            var result = (decimal)(BslNumericValue)func.DynamicInvoke(args);
            result.Should().Be(6);
        }
        
        [Fact]
        public void Can_Do_ForEachLoop()
        {
            var tm = new DefaultTypeManager();
            var arrayType = tm.RegisterClass(typeof(ArrayImpl));
            
            var block = new CompiledBlock(default);
            block.Parameters.Insert("Результат", new BslTypeValue(BasicTypes.Number));
            block.Parameters.Insert("П", new BslTypeValue(arrayType));
            
            block.CodeBlock = 
                "Для Каждого Ф Из П Цикл " +
                "Если Ф = 4 Тогда Продолжить; КонецЕсли; " +
                "Если Ф = 5 Тогда Прервать; КонецЕсли;" +
                "Результат = Результат + Ф;" +
                "КонецЦикла;" +
                "Возврат Результат;";
            var expression = block.MakeExpression();
            var func = expression.Compile();

            var inArray = new ArrayImpl();

            inArray.Add(ValueFactory.Create(1));
            inArray.Add(ValueFactory.Create(2));
            inArray.Add(ValueFactory.Create(3));
            inArray.Add(ValueFactory.Create(4));
            inArray.Add(ValueFactory.Create(5));
            
            var args = new object[] { decimal.Zero, inArray };
            var result = (decimal)(BslNumericValue)func.DynamicInvoke(args);
            result.Should().Be(6);
        }

        [Fact]
        public void Can_Do_TryExcept()
        {
            var block = new CompiledBlock(default);
            block.Parameters.Insert("Ф", new BslTypeValue(BasicTypes.Number));
            block.CodeBlock = 
                "Попытка Если Ф = 1 Тогда Возврат 1; КонецЕсли;" +
                "ВызватьИсключение 123; " +
                "Исключение Возврат 2; КонецПопытки;";
            var expression = block.MakeExpression();

            expression.Body.As<BlockExpression>().Expressions[0].NodeType.Should().Be(ExpressionType.Try);
            
            var func = expression.Compile();
            
            ((decimal)(BslNumericValue)func.DynamicInvoke(new object[] { decimal.One })).Should().Be(1);
            ((decimal)(BslNumericValue)func.DynamicInvoke(new object[] { decimal.Zero })).Should().Be(2);
        }

        [Fact]
        public void CanCallGlobalFunctions()
        {
            var block = new CompiledBlock(default);
            var context = new StandardGlobalContext();
            block.Symbols = new SymbolTable();
            block.Symbols.PushScope(SymbolScope.FromObject(context));
            block.CodeBlock = "Возврат ТекущаяУниверсальнаяДатаВМиллисекундах();";
            
            var func = block.CreateDelegate<Func<BslValue>>();
            var time = (decimal)(BslNumericValue) func();
            time.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Can_Do_Eratosthenes()
        {
            var code = @"времяНачала = ТекущаяУниверсальнаяДатаВМиллисекундах();
            Для индекс = 2 По Н Цикл
                Если Массив[индекс] Тогда
                    квадрат = индекс * индекс;
                    Если квадрат <= Н Тогда
                        м = квадрат;
                        Пока м <= Н Цикл
                            Массив[м] = Ложь;
                            м = м + индекс;
                        КонецЦикла;
                    КонецЕсли;
                КонецЕсли;
            КонецЦикла;

            времяОкончания = ТекущаяУниверсальнаяДатаВМиллисекундах();
            Возврат (времяОкончания - времяНачала)/1000";

            var tm = new DefaultTypeManager();
            tm.RegisterClass(typeof(ArrayImpl));
            var blockCompiler = new CompiledBlock(default);

            var N = 5000;
            var arr = new ArrayImpl();
            for (int i = 0; i < N; i++)
            {
                if(i < 2)
                    arr.Add(BslBooleanValue.False);
                
                arr.Add(BslBooleanValue.True);
            }

            var arrayType = tm.GetTypeByFrameworkType(typeof(ArrayImpl));
            
            blockCompiler.Parameters.Insert("Н", new BslTypeValue(BasicTypes.Number));
            blockCompiler.Parameters.Insert("Массив", new BslTypeValue(arrayType));
            blockCompiler.Symbols = new SymbolTable();
            blockCompiler.Symbols.PushScope(SymbolScope.FromObject(new StandardGlobalContext()));
            blockCompiler.CodeBlock = code;
            
            var lambda = blockCompiler.MakeExpression();
            var eratosphenes = blockCompiler.CreateDelegate<Func<decimal, ArrayImpl, BslValue>>();
            var time = (decimal)(BslNumericValue)eratosphenes(N, arr);
            time.Should().NotBe(0);
        }

        [Fact]
        public void Undefined_To_Object_Makes_NullObject()
        {
            var tm = new DefaultTypeManager();
            var arrayType = tm.RegisterClass(typeof(ArrayImpl));
            var blockCompiler = new CompiledBlock(default);
            blockCompiler.Parameters.Insert("А", new BslTypeValue(arrayType));
            blockCompiler.CodeBlock = "А = Неопределено";

            var lambda = blockCompiler.MakeExpression();
            var assignment = lambda.Body.As<BlockExpression>().Expressions[0].As<BinaryExpression>();
            assignment.Right.Should().BeAssignableTo<DefaultExpression>();
            assignment.Type.Should().Be(typeof(ArrayImpl));
        }

        [Fact]
        public void Can_Call_Parameterless_Constructor()
        {
            var block = GetCompiler((tm, s) => tm.RegisterClass(typeof(ArrayImpl)));
            block.CodeBlock = "Возврат Новый Массив";

            var func = block.CreateDelegate<Func<BslValue>>();
            var arrayBsl = func();

            arrayBsl.Should().BeOfType<ArrayImpl>();
        }
        
        [Fact]
        public void Can_Call_Parameterized_Constructor_With_AllArgs()
        {
            var tm = new DefaultTypeManager();
            tm.RegisterClass(typeof(ArrayImpl));

            var services = new TinyIocImplementation();
            services.Register<ITypeManager>(tm);
            
            var block = new CompiledBlock(services.CreateContainer());
            block.Parameters.Insert("Размер", new BslTypeValue(BasicTypes.Number));
            block.CodeBlock = "Возврат Новый Массив(Размер)";

            var func = block.CreateDelegate<Func<decimal, BslValue>>();
            var arrayBsl = func(15);

            arrayBsl.As<ArrayImpl>().Count().Should().Be(15);
        }
        
        [Fact]
        public void Can_Call_Parameterized_Constructor_With_SkippedArgs()
        {
            var tm = new DefaultTypeManager();
            tm.RegisterClass(typeof(TextReadImpl));

            var services = new TinyIocImplementation();
            services.Register<ITypeManager>(tm);
            
            var block = new CompiledBlock(services.CreateContainer());
            block.Parameters.Insert("Путь", new BslTypeValue(BasicTypes.String));
            block.CodeBlock = "Возврат Новый ЧтениеТекста(Путь,,,,Истина)";

            var func = block.CreateDelegate<Func<string, BslValue>>();
            var tempFile = System.IO.Path.GetTempFileName();
            try
            {
                using var reader = (TextReadImpl)func(tempFile);
                reader.Close();
            }
            finally
            {
                System.IO.File.Delete(tempFile);
            }
        }

        [Fact]
        public void Can_Call_TypeConversions()
        {
            var block = new CompiledBlock(default);
            block.CodeBlock = 
                @"БулевТип = Истина;
                ЧисловойТип = 1;
                БулевТип = Булево(ЧисловойТип);";

            var l = block.MakeExpression();
            l.Body.As<BlockExpression>().Expressions[2].As<BinaryExpression>()
                .Right.NodeType
                .Should()
                .Be(ExpressionType.Call);
        }
        
        [Fact]
        public void Can_Call_ParameterlessFunctions()
        {
            var block = new CompiledBlock(default);
            block.CodeBlock = "А = ТекущаяДата()";

            var l = block.MakeExpression();
            var statement = l.Body.As<BlockExpression>().Expressions[0].As<BinaryExpression>();

            statement.Right.NodeType.Should().Be(ExpressionType.Call);
        }

        [Fact]
        public void Can_Call_Member_Procedures()
        {
            var tm = new DefaultTypeManager();
            var arrayType = tm.RegisterClass(typeof(ArrayImpl));
            
            var block = new CompiledBlock(default);
            block.Parameters.Insert("Массив", new BslTypeValue(arrayType));
            block.CodeBlock = "Массив.Добавить(1); Массив.Добавить(2);";

            var method = block.CreateDelegate<Func<ArrayImpl, BslValue>>();
            var array = new ArrayImpl();
            method(array);

            array.Should().HaveCount(2);
        }
        
        [Fact]
        public void Can_Call_Member_Procedures_On_Dynamics()
        {
            var tm = new DefaultTypeManager();
            var arrayType = tm.RegisterClass(typeof(ArrayImpl));
            
            var block = GetCompiler(tm);
            block.Parameters.Insert("Массив", new BslTypeValue(arrayType));
            block.CodeBlock = "Массив.Добавить(Новый Массив); Массив[0].Добавить(2)";

            var l = block.MakeExpression();
            var method = block.CreateDelegate<Func<ArrayImpl, BslValue>>();
            var array = new ArrayImpl();
            method(array);

            array.Should().HaveCount(1);
            array.Get(0).As<ArrayImpl>().Should().HaveCount(1);
        }
        
        [Fact]
        public void Can_Call_Member_Procedures_With_Defaults()
        {
            var tm = new DefaultTypeManager();
            var arrayType = tm.RegisterClass(typeof(ArrayImpl));
            var services = new TinyIocImplementation();
            services.Register<ITypeManager>(tm);
            var block = new CompiledBlock(services);
            block.Parameters.Insert("Массив", new BslTypeValue(arrayType));
            block.CodeBlock = "Массив.Добавить();";

            var method = block.CreateDelegate<Func<ArrayImpl, BslValue>>();
            var array = new ArrayImpl();
            method(array);

            array.Should().HaveCount(1);
        }
        [Fact]
        public void Can_Call_Member_Functions()
        {
            var tm = new DefaultTypeManager();
            var arrayType = tm.RegisterClass(typeof(ArrayImpl));
            
            var block = new CompiledBlock(default);
            block.Parameters.Insert("Массив", new BslTypeValue(arrayType));
            block.CodeBlock = "Массив.Добавить(1); Массив.Добавить(2); А = Массив.Количество()";

            var lambda = block.MakeExpression();

            var lastAssignment = lambda.Body.As<BlockExpression>().Expressions[^2].As<BinaryExpression>();
            lastAssignment.Right.Type.Should().Be(typeof(decimal));
        }
        
        [Fact]
        public void Can_Do_PropRead_Static()
        {
            var tm = new DefaultTypeManager();
            var objectType = tm.RegisterClass(typeof(ValueTable));

            var block = new CompiledBlock(default);
            block.Parameters.Insert("Ф", new BslTypeValue(objectType));
            block.CodeBlock = 
                "Возврат Ф.Колонки.Количество();";
            var expression = block.MakeExpression();

            var func = expression.Compile();

            var testData = new ValueTable();
            testData.Columns.Add("Колонка1");
            testData.Columns.Add("Колонка2");

            ((decimal)(BslNumericValue)func.DynamicInvoke(new object[] { testData })).Should().Be(2M);
        }

        [Fact]
        public void Can_Do_PropRead_Dynamic()
        {
            var tm = new DefaultTypeManager();
            var objectType = tm.RegisterClass(typeof(StructureImpl));

            var block = new CompiledBlock(default);
            block.Parameters.Insert("Ф", new BslTypeValue(objectType));
            block.CodeBlock = 
                "Возврат Ф.Свойство1.ВложенноеСвойство1;";
            var expression = block.MakeExpression();

            var func = expression.Compile();

            var innerTestData = new StructureImpl();
            innerTestData.Insert("ВложенноеСвойство1", ValueFactory.Create(2M));

            var testData = new StructureImpl();
            testData.Insert("Свойство1", innerTestData);

            ((decimal)(BslNumericValue)func.DynamicInvoke(new object[] { testData })).Should().Be(2M);
        }

        [Fact]
        public void Can_Do_PropWrite_Dynamic()
        {
            var tm = new DefaultTypeManager();
            var objectType = tm.RegisterClass(typeof(StructureImpl));

            var block = new CompiledBlock(default);
            block.Parameters.Insert("Ф", new BslTypeValue(objectType));
            block.Parameters.Insert("П", new BslTypeValue(objectType));
            block.Parameters.Insert("Ж", new BslTypeValue(BasicTypes.Number));
            block.CodeBlock = 
                "Ф.Свойство1 = П;" +
                "Ф.Свойство1.ВложенноеСвойство1 = Ж;" +
                "Возврат Ф.Свойство1.ВложенноеСвойство1;";
            var expression = block.MakeExpression();

            var func = expression.Compile();

            var innerTestData = new StructureImpl();
            innerTestData.Insert("ВложенноеСвойство1", ValueFactory.Create(1M));

            var testData = new StructureImpl();
            testData.Insert("Свойство1", innerTestData);

            ((decimal)(BslNumericValue)func.DynamicInvoke(new object[] { testData, innerTestData, 2M })).Should().Be(2M);
        }
        
        [Fact]
        public void Can_Do_PropWrite_Static()
        {
            var tm = new DefaultTypeManager();
            tm.RegisterClass(typeof(ValueListImpl));
            var objectType = tm.RegisterClass(typeof(ValueListItem));

            var block = new CompiledBlock(default);
            block.Parameters.Insert("Ф", new BslTypeValue(objectType));
            block.Parameters.Insert("НовоеЗначение", new BslTypeValue(BasicTypes.Number));
            block.CodeBlock = 
                "Ф.Значение = НовоеЗначение; Возврат Ф.Значение";
            var expression = block.MakeExpression();

            var func = expression.Compile();

            var testStructure = new ValueListImpl();
            testStructure.Add(ValueFactory.Create(1M));

            var testData = testStructure.FirstOrDefault();

            ((decimal) (BslNumericValue) func.DynamicInvoke(new object[] {testData, 2M}))
                .Should().Be(2M);
        }

        [Fact]
        public void ExceptionInfo_ReturnsUndefined_OutsideOfCatch()
        {
            var block = GetCompiler(new DefaultTypeManager());
            block.CodeBlock = "А = ИнформацияОбОшибке();";

            var lambda = block.MakeExpression();
            lambda.Body.As<BlockExpression>().Expressions[0].Type.Should().Be(typeof(BslValue));
        }
        
        [Fact]
        public void ExceptionInfo_ReturnsClass_InCatch()
        {
            var block = GetCompiler(new DefaultTypeManager());
            block.CodeBlock = "Попытка\n" +
                              "  ;\n" +
                              "Исключение\n" +
                              " А = ИнформацияОбОшибке();\n" +
                              "КонецПопытки;";
            
            var lambda = block.MakeExpression();
            var tryBlock = lambda.Body.As<BlockExpression>().Expressions[0].As<TryExpression>();

            tryBlock.Handlers.First().Body.As<BlockExpression>().Expressions[0].Type
                .Should()
                .Be(typeof(ExceptionInfoClass));
        }
        
        [Fact]
        public void LazyBoolOr()
        {
            var tm = new DefaultTypeManager();
            var objectType = tm.RegisterClass(typeof(StructureImpl));
            var block = GetCompiler(new DefaultTypeManager());
            block.Parameters.Insert("Ф", new BslTypeValue(objectType));
            block.CodeBlock = "Возврат Истина Или Ф;"; // Ф не должен быть вычислен
            
            var lambda = block.MakeExpression();
            var func = lambda.Compile();

            var testData = new StructureImpl();

            ((bool)(BslBooleanValue)func.DynamicInvoke(new object[] { testData })).Should().Be(true);
        }
        
        [Fact]
        public void LazyBoolAnd()
        {
            var tm = new DefaultTypeManager();
            var objectType = tm.RegisterClass(typeof(StructureImpl));
            var block = GetCompiler(new DefaultTypeManager());
            block.Parameters.Insert("Ф", new BslTypeValue(objectType));
            block.CodeBlock = "Возврат Ложь И Ф;"; // Ф не должен быть вычислен
            
            var lambda = block.MakeExpression();
            var func = lambda.Compile();

            var testData = new StructureImpl();

            ((bool)(BslBooleanValue)func.DynamicInvoke(new object[] { testData })).Should().Be(false);
        }
    }
}