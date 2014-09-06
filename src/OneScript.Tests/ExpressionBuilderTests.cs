using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Scripting;
using System;
using System.Collections.Generic;


namespace OneScript.Tests
{
    [TestClass]
    public class ExpressionBuilderTests
    {
        [TestMethod]
        public void ExpressionBuilder_CallSequence()
        {
            var builder = new CallSequenceLogger();
            Lexer lexer = new Lexer();
            var ctx = new CompilerContext();
            var exprBuilder = new ExpressionBuilder(builder, new LexemExtractor(lexer), ctx);

            lexer.Code = "А+1*Б(1,2,3)-F.D;";

            exprBuilder.Build(Token.Semicolon);

            var expectedSequence = new string[]
            {
                "BeginExpression",
                "BuildReadVariable",
                "AddOperation",
                "ReadConstant",
                "AddOperation",
                "BeginMethodCall",
                "ReadConstant",
                "AddArgument",
                "ReadConstant",
                "AddArgument",
                "ReadConstant",
                "EndMethodCall",
                "AddOperation",
                "BuildReadVariable",
                "BuildGetReference",
                "EndExpression"
            };

            Assert.IsTrue(expectedSequence.Length == builder.Sequence.Count);
            for (int i = 0; i < expectedSequence.Length; i++)
            {
                Assert.IsTrue(builder.Sequence[i] == expectedSequence[i]);
            }

        }

        class CallSequenceLogger : IModuleBuilder
        {
            List<string> _callSequence = new List<string>();

            public List<string> Sequence
            {
                get
                {
                    return _callSequence;
                }
            }

            private void SetCall(string name)
            {
                _callSequence.Add(name);
            }

            public void BeginModule(ICompilerContext context)
            {
                SetCall("BeginModule");
            }

            public void CompleteModule()
            {
                SetCall("CompleteModule");
            }

            public void SnapToCodeLine(int line)
            {
                SetCall("SnapToCodeLine");
            }

            public void BuildVariable(string name)
            {
                SetCall("BuildVariable"); 
            }

            public void BuildExportVariable(string name)
            {
                SetCall("BuildExportVariable");
            }

            public void BuildLoadVariable(SymbolBinding binding)
            {
                SetCall("BuildLoadVariable");
            }

            public void BuildReadConstant(ConstDefinition constDef)
            {
                SetCall("BuildReadConstant");
            }

            public void BuildGetReference(ConstDefinition constDef)
            {
                SetCall("BuildGetReference");
            }

            public void WriteReference()
            {
                SetCall("WriteReference");
            }

            public void OnError(CompilerErrorEventArgs errorInfo)
            {
            }


            public void BeginExpression()
            {
                throw new NotImplementedException();
            }

            public void EndExpression()
            {
                throw new NotImplementedException();
            }

            public void BeginMethodCall(string methodName, bool asFunction)
            {
                throw new NotImplementedException();
            }

            public void AddArgument()
            {
                throw new NotImplementedException();
            }

            public void EndMethodCall()
            {
                throw new NotImplementedException();
            }

            public void BeginIndexAccess()
            {
                throw new NotImplementedException();
            }

            public void EndIndexAccess()
            {
                throw new NotImplementedException();
            }


            public void AddOperation(Token operatorToken)
            {
                throw new NotImplementedException();
            }


            public void BuildReadVariable(SymbolBinding binding)
            {
                throw new NotImplementedException();
            }
        }

        class LexemExtractor : ILexemExtractor
        {
            Lexer _lexer;
            Lexem _last;

            public LexemExtractor(Lexer lexer)
            {
                _lexer = lexer;
                _last = Lexem.Empty();
            }

            public Lexem LastExtractedLexem
            {
                get { return _last; }
            }

            public void NextLexem()
            {
                _last = _lexer.NextLexem();
            }
        }
    }
}
