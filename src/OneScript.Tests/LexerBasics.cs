using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Language;

namespace OneScript.Tests
{
    [TestClass]
    public class LexerBasics
    {
        [TestMethod]
        public void Empty_Lexem_Is_Empty()
        {
            var lex = Lexem.Empty();
            Assert.IsTrue(lex.Type == LexemType.NotALexem);
            Assert.IsTrue(lex.Token == Token.NotAToken);
            Assert.IsTrue(lex.Content == null);
        }

        [TestMethod]
        public void EndOfText_Lexem_Is_Correct()
        {
            var lex = Lexem.EndOfText();
            Assert.IsTrue(lex.Type == LexemType.EndOfText);
            Assert.IsTrue(lex.Token == Token.EndOfText);
            Assert.IsTrue(lex.Content == null);
        }

        [TestMethod]
        public void Special_Characters()
        {
            Assert.IsTrue(SpecialChars.IsOperatorChar('+'));
            Assert.IsTrue(SpecialChars.IsOperatorChar('-'));
            Assert.IsTrue(SpecialChars.IsOperatorChar('*'));
            Assert.IsTrue(SpecialChars.IsOperatorChar('/'));
            Assert.IsTrue(SpecialChars.IsOperatorChar('<'));
            Assert.IsTrue(SpecialChars.IsOperatorChar('>'));
            Assert.IsTrue(SpecialChars.IsOperatorChar('='));
            Assert.IsTrue(SpecialChars.IsOperatorChar('%'));
            Assert.IsTrue(SpecialChars.IsOperatorChar('('));
            Assert.IsTrue(SpecialChars.IsOperatorChar(')'));
            Assert.IsTrue(SpecialChars.IsOperatorChar('.'));
            Assert.IsTrue(SpecialChars.IsOperatorChar(','));
            Assert.IsTrue(SpecialChars.IsOperatorChar('['));
            Assert.IsTrue(SpecialChars.IsOperatorChar(']'));
        }

        [TestMethod]
        public void Operation_Priority()
        {
            Assert.IsTrue(LanguageDef.GetPriority(Token.Plus) == 5);
            Assert.IsTrue(LanguageDef.GetPriority(Token.Minus) == 5);
            Assert.IsTrue(LanguageDef.GetPriority(Token.UnaryMinus) == 5);
            Assert.IsTrue(LanguageDef.GetPriority(Token.Multiply) == 6);
            Assert.IsTrue(LanguageDef.GetPriority(Token.Division) == 6);
            Assert.IsTrue(LanguageDef.GetPriority(Token.Modulo) == 6);

            Assert.IsTrue(LanguageDef.GetPriority(Token.Or) == 1);
            Assert.IsTrue(LanguageDef.GetPriority(Token.And) == 2);
            Assert.IsTrue(LanguageDef.GetPriority(Token.Not) == 3);

            Assert.IsTrue(LanguageDef.GetPriority(Token.Equal) == 4);
            Assert.IsTrue(LanguageDef.GetPriority(Token.MoreThan) == 4);
            Assert.IsTrue(LanguageDef.GetPriority(Token.LessThan) == 4);
            Assert.IsTrue(LanguageDef.GetPriority(Token.MoreOrEqual) == 4);
            Assert.IsTrue(LanguageDef.GetPriority(Token.LessOrEqual) == 4);
            Assert.IsTrue(LanguageDef.GetPriority(Token.NotEqual) == 4);
        }

        [TestMethod]
        public void IsBuiltIn_Function()
        {
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.Question));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.Bool));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.Number));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.Str));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.Date));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.StrLen));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.TrimL));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.TrimR));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.TrimLR));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.Left));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.Right));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.Mid));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.StrPos));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.UCase));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.LCase));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.Chr));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.ChrCode));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.EmptyStr));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.StrReplace));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.Year));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.Month));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.Day));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.Hour));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.Minute));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.Second));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.BegOfYear));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.BegOfMonth));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.BegOfDay));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.BegOfHour));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.BegOfMinute));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.BegOfQuarter));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.EndOfYear));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.EndOfMonth));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.EndOfDay));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.EndOfHour));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.EndOfMinute));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.EndOfQuarter));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.WeekOfYear));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.DayOfYear));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.DayOfWeek));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.AddMonth));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.CurrentDate));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.Integer));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.Round));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.Pow));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.Sqrt));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.ExceptionInfo));
            Assert.IsTrue(LanguageDef.IsBuiltInFunction(Token.ExceptionDescr));
        }

        [TestMethod]
        public void All_Tokens()
        {
            Assert.IsTrue(LanguageDef.GetToken("если") == Token.If);
            Assert.IsTrue(LanguageDef.GetToken("тогда") == Token.Then);
            Assert.IsTrue(LanguageDef.GetToken("иначе") == Token.Else);
            Assert.IsTrue(LanguageDef.GetToken("иначеесли") == Token.ElseIf);
            Assert.IsTrue(LanguageDef.GetToken("конецесли") == Token.EndIf);
            Assert.IsTrue(LanguageDef.GetToken("перем") == Token.VarDef);
            Assert.IsTrue(LanguageDef.GetToken("знач") == Token.ByValParam);
            Assert.IsTrue(LanguageDef.GetToken("процедура") == Token.Procedure);
            Assert.IsTrue(LanguageDef.GetToken("конецпроцедуры") == Token.EndProcedure);
            Assert.IsTrue(LanguageDef.GetToken("функция") == Token.Function);
            Assert.IsTrue(LanguageDef.GetToken("конецфункции") == Token.EndFunction);
            Assert.IsTrue(LanguageDef.GetToken("для") == Token.For);
            Assert.IsTrue(LanguageDef.GetToken("каждого") == Token.Each);
            Assert.IsTrue(LanguageDef.GetToken("из") == Token.In);
            Assert.IsTrue(LanguageDef.GetToken("по") == Token.To);
            Assert.IsTrue(LanguageDef.GetToken("пока") == Token.While);
            Assert.IsTrue(LanguageDef.GetToken("цикл") == Token.Loop);
            Assert.IsTrue(LanguageDef.GetToken("конеццикла") == Token.EndLoop);
            Assert.IsTrue(LanguageDef.GetToken("возврат") == Token.Return);
            Assert.IsTrue(LanguageDef.GetToken("продолжить") == Token.Continue);
            Assert.IsTrue(LanguageDef.GetToken("прервать") == Token.Break);
            Assert.IsTrue(LanguageDef.GetToken("попытка") == Token.Try);
            Assert.IsTrue(LanguageDef.GetToken("исключение") == Token.Exception);
            Assert.IsTrue(LanguageDef.GetToken("вызватьисключение") == Token.RaiseException);
            Assert.IsTrue(LanguageDef.GetToken("конецпопытки") == Token.EndTry);
            Assert.IsTrue(LanguageDef.GetToken("новый") == Token.NewObject);
            Assert.IsTrue(LanguageDef.GetToken("экспорт") == Token.Export);

            Assert.IsTrue(LanguageDef.GetToken("if") == Token.If);
            Assert.IsTrue(LanguageDef.GetToken("then") == Token.Then);
            Assert.IsTrue(LanguageDef.GetToken("else") == Token.Else);
            Assert.IsTrue(LanguageDef.GetToken("elseif") == Token.ElseIf);
            Assert.IsTrue(LanguageDef.GetToken("endif") == Token.EndIf);
            Assert.IsTrue(LanguageDef.GetToken("var") == Token.VarDef);
            Assert.IsTrue(LanguageDef.GetToken("val") == Token.ByValParam);
            Assert.IsTrue(LanguageDef.GetToken("procedure") == Token.Procedure);
            Assert.IsTrue(LanguageDef.GetToken("endprocedure") == Token.EndProcedure);
            Assert.IsTrue(LanguageDef.GetToken("function") == Token.Function);
            Assert.IsTrue(LanguageDef.GetToken("endfunction") == Token.EndFunction);
            Assert.IsTrue(LanguageDef.GetToken("for") == Token.For);
            Assert.IsTrue(LanguageDef.GetToken("each") == Token.Each);
            Assert.IsTrue(LanguageDef.GetToken("from") == Token.In);
            Assert.IsTrue(LanguageDef.GetToken("to") == Token.To);
            Assert.IsTrue(LanguageDef.GetToken("while") == Token.While);
            Assert.IsTrue(LanguageDef.GetToken("do") == Token.Loop);
            Assert.IsTrue(LanguageDef.GetToken("enddo") == Token.EndLoop);
            Assert.IsTrue(LanguageDef.GetToken("return") == Token.Return);
            Assert.IsTrue(LanguageDef.GetToken("contınue") == Token.Continue);
            Assert.IsTrue(LanguageDef.GetToken("break") == Token.Break);
            Assert.IsTrue(LanguageDef.GetToken("try") == Token.Try);
            Assert.IsTrue(LanguageDef.GetToken("exception") == Token.Exception);
            Assert.IsTrue(LanguageDef.GetToken("raise") == Token.RaiseException);
            Assert.IsTrue(LanguageDef.GetToken("endtry") == Token.EndTry);
            Assert.IsTrue(LanguageDef.GetToken("new") == Token.NewObject);
            Assert.IsTrue(LanguageDef.GetToken("export") == Token.Export);

            Assert.IsTrue(LanguageDef.GetToken("+") == Token.Plus);
            Assert.IsTrue(LanguageDef.GetToken("-") == Token.Minus);
            Assert.IsTrue(LanguageDef.GetToken("*") == Token.Multiply);
            Assert.IsTrue(LanguageDef.GetToken("/") == Token.Division);
            Assert.IsTrue(LanguageDef.GetToken("<") == Token.LessThan);
            Assert.IsTrue(LanguageDef.GetToken("<=") == Token.LessOrEqual);
            Assert.IsTrue(LanguageDef.GetToken(">") == Token.MoreThan);
            Assert.IsTrue(LanguageDef.GetToken(">=") == Token.MoreOrEqual);
            Assert.IsTrue(LanguageDef.GetToken("<>") == Token.NotEqual);
            Assert.IsTrue(LanguageDef.GetToken("%") == Token.Modulo);
            Assert.IsTrue(LanguageDef.GetToken("и") == Token.And);
            Assert.IsTrue(LanguageDef.GetToken("или") == Token.Or);
            Assert.IsTrue(LanguageDef.GetToken("не") == Token.Not);
            Assert.IsTrue(LanguageDef.GetToken("(") == Token.OpenPar);
            Assert.IsTrue(LanguageDef.GetToken(")") == Token.ClosePar);
            Assert.IsTrue(LanguageDef.GetToken("[") == Token.OpenBracket);
            Assert.IsTrue(LanguageDef.GetToken("]") == Token.CloseBracket);
            Assert.IsTrue(LanguageDef.GetToken(".") == Token.Dot);
            Assert.IsTrue(LanguageDef.GetToken(",") == Token.Comma);
            Assert.IsTrue(LanguageDef.GetToken("=") == Token.Equal);
            Assert.IsTrue(LanguageDef.GetToken(";") == Token.Semicolon);

            Assert.IsTrue(LanguageDef.GetToken("?") == Token.Question);
            Assert.IsTrue(LanguageDef.GetToken("булево") == Token.Bool);
            Assert.IsTrue(LanguageDef.GetToken("число") == Token.Number);
            Assert.IsTrue(LanguageDef.GetToken("строка") == Token.Str);
            Assert.IsTrue(LanguageDef.GetToken("дата") == Token.Date);

            Assert.IsTrue(LanguageDef.GetToken("стрдлина") == Token.StrLen);
            Assert.IsTrue(LanguageDef.GetToken("сокрл") == Token.TrimL);
            Assert.IsTrue(LanguageDef.GetToken("сокрп") == Token.TrimR);
            Assert.IsTrue(LanguageDef.GetToken("сокрлп") == Token.TrimLR);
            Assert.IsTrue(LanguageDef.GetToken("лев") == Token.Left);
            Assert.IsTrue(LanguageDef.GetToken("прав") == Token.Right);
            Assert.IsTrue(LanguageDef.GetToken("сред") == Token.Mid);
            Assert.IsTrue(LanguageDef.GetToken("найти") == Token.StrPos);
            Assert.IsTrue(LanguageDef.GetToken("врег") == Token.UCase);
            Assert.IsTrue(LanguageDef.GetToken("нрег") == Token.LCase);
            Assert.IsTrue(LanguageDef.GetToken("символ") == Token.Chr);
            Assert.IsTrue(LanguageDef.GetToken("кодсимвола") == Token.ChrCode);
            Assert.IsTrue(LanguageDef.GetToken("пустаястрока") == Token.EmptyStr);
            Assert.IsTrue(LanguageDef.GetToken("стрзаменить") == Token.StrReplace);

            Assert.IsTrue(LanguageDef.GetToken("год") == Token.Year);
            Assert.IsTrue(LanguageDef.GetToken("месяц") == Token.Month);
            Assert.IsTrue(LanguageDef.GetToken("день") == Token.Day);
            Assert.IsTrue(LanguageDef.GetToken("час") == Token.Hour);
            Assert.IsTrue(LanguageDef.GetToken("минута") == Token.Minute);
            Assert.IsTrue(LanguageDef.GetToken("секунда") == Token.Second);
            Assert.IsTrue(LanguageDef.GetToken("началогода") == Token.BegOfYear);
            Assert.IsTrue(LanguageDef.GetToken("началомесяца") == Token.BegOfMonth);
            Assert.IsTrue(LanguageDef.GetToken("началодня") == Token.BegOfDay);
            Assert.IsTrue(LanguageDef.GetToken("началочаса") == Token.BegOfHour);
            Assert.IsTrue(LanguageDef.GetToken("началоминуты") == Token.BegOfMinute);
            Assert.IsTrue(LanguageDef.GetToken("началоквартала") == Token.BegOfQuarter);
            Assert.IsTrue(LanguageDef.GetToken("конецгода") == Token.EndOfYear);
            Assert.IsTrue(LanguageDef.GetToken("конецмесяца") == Token.EndOfMonth);
            Assert.IsTrue(LanguageDef.GetToken("конецдня") == Token.EndOfDay);
            Assert.IsTrue(LanguageDef.GetToken("конецчаса") == Token.EndOfHour);
            Assert.IsTrue(LanguageDef.GetToken("конецминуты") == Token.EndOfMinute);
            Assert.IsTrue(LanguageDef.GetToken("конецквартала") == Token.EndOfQuarter);
            Assert.IsTrue(LanguageDef.GetToken("неделягода") == Token.WeekOfYear);
            Assert.IsTrue(LanguageDef.GetToken("деньгода") == Token.DayOfYear);
            Assert.IsTrue(LanguageDef.GetToken("деньнедели") == Token.DayOfWeek);
            Assert.IsTrue(LanguageDef.GetToken("добавитьмесяц") == Token.AddMonth);
            Assert.IsTrue(LanguageDef.GetToken("текущаядата") == Token.CurrentDate);
            Assert.IsTrue(LanguageDef.GetToken("цел") == Token.Integer);
            Assert.IsTrue(LanguageDef.GetToken("окр") == Token.Round);
            Assert.IsTrue(LanguageDef.GetToken("pow") == Token.Pow);
            Assert.IsTrue(LanguageDef.GetToken("sqrt") == Token.Sqrt);
            Assert.IsTrue(LanguageDef.GetToken("информацияобошибке") == Token.ExceptionInfo);
            Assert.IsTrue(LanguageDef.GetToken("описаниеошибки") == Token.ExceptionDescr);
        }

        [TestMethod]
        public void Keywords_Which_Begins_New_Statement()
        {
            Assert.IsTrue(LanguageDef.IsBeginOfStatement(Token.VarDef));
            Assert.IsTrue(LanguageDef.IsBeginOfStatement(Token.If));
            Assert.IsTrue(LanguageDef.IsBeginOfStatement(Token.For));
            Assert.IsTrue(LanguageDef.IsBeginOfStatement(Token.While));
            Assert.IsTrue(LanguageDef.IsBeginOfStatement(Token.Try));
        }
    }
}
