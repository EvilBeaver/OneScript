/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;
using Xunit;

namespace OneScript.Language.Tests
{
    public class LexerBasicsTests
    {
        [Fact]
        public void Empty_Lexem_Is_Empty()
        {
            var lex = Lexem.Empty();
            Assert.True(lex.Type == LexemType.NotALexem);
            Assert.True(lex.Token == Token.NotAToken);
            Assert.True(lex.Content == null);
        }

        [Fact]
        public void EndOfText_Lexem_Is_Correct()
        {
            var lex = Lexem.EndOfText();
            Assert.True(lex.Type == LexemType.EndOfText);
            Assert.True(lex.Token == Token.EndOfText);
            Assert.True(lex.Content == null);
        }

        [Fact]
        public void IteratorReturnsCorrectLine()
        {
            var code = "\r\nF = 1;\r\nD = 2;\r\nX = 3;\r\n";
            var iterator = new SourceCodeIterator(code);

            Assert.Equal(1, iterator.CurrentLine);
            Assert.Equal(-1, iterator.CurrentColumn);

            iterator.MoveToContent();

            Assert.Equal(2, iterator.CurrentLine);
            Assert.Equal(1, iterator.CurrentColumn);
            Assert.Equal("F = 1;\r", iterator.GetCodeLine(2));

            iterator.MoveNext();
            Assert.Equal(2, iterator.CurrentColumn);
            iterator.MoveNext();
            Assert.Equal(3, iterator.CurrentColumn);
            Assert.Equal('=', iterator.CurrentSymbol);
            while (iterator.CurrentSymbol != 'D' && iterator.MoveNext())
                ;

            Assert.Equal(1, iterator.CurrentColumn);
            Assert.Equal(3, iterator.CurrentLine);

        }

        [Fact]
        public void Special_Characters()
        {
            Assert.True(SpecialChars.IsOperatorChar('+'));
            Assert.True(SpecialChars.IsOperatorChar('-'));
            Assert.True(SpecialChars.IsOperatorChar('*'));
            Assert.True(SpecialChars.IsOperatorChar('/'));
            Assert.True(SpecialChars.IsOperatorChar('<'));
            Assert.True(SpecialChars.IsOperatorChar('>'));
            Assert.True(SpecialChars.IsOperatorChar('='));
            Assert.True(SpecialChars.IsOperatorChar('%'));
            Assert.True(SpecialChars.IsOperatorChar('('));
            Assert.True(SpecialChars.IsOperatorChar(')'));
            Assert.True(SpecialChars.IsOperatorChar('.'));
            Assert.True(SpecialChars.IsOperatorChar(','));
            Assert.True(SpecialChars.IsOperatorChar('['));
            Assert.True(SpecialChars.IsOperatorChar(']'));
        }

        [Fact]
        public void Operation_Priority()
        {
            Assert.True(LanguageDef.GetPriority(Token.Plus) == 5);
            Assert.True(LanguageDef.GetPriority(Token.Minus) == 5);
            Assert.True(LanguageDef.GetPriority(Token.UnaryMinus) == 7);
            Assert.True(LanguageDef.GetPriority(Token.UnaryPlus) == 7);
            Assert.True(LanguageDef.GetPriority(Token.Multiply) == 6);
            Assert.True(LanguageDef.GetPriority(Token.Division) == 6);
            Assert.True(LanguageDef.GetPriority(Token.Modulo) == 6);

            Assert.True(LanguageDef.GetPriority(Token.Or) == 1);
            Assert.True(LanguageDef.GetPriority(Token.And) == 2);
            Assert.True(LanguageDef.GetPriority(Token.Not) == 3);

            Assert.True(LanguageDef.GetPriority(Token.Equal) == 4);
            Assert.True(LanguageDef.GetPriority(Token.MoreThan) == 4);
            Assert.True(LanguageDef.GetPriority(Token.LessThan) == 4);
            Assert.True(LanguageDef.GetPriority(Token.MoreOrEqual) == 4);
            Assert.True(LanguageDef.GetPriority(Token.LessOrEqual) == 4);
            Assert.True(LanguageDef.GetPriority(Token.NotEqual) == 4);
        }

        [Fact]
        public void IsBuiltIn_Function()
        {
            Assert.True(LanguageDef.IsBuiltInFunction(Token.Bool));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.Number));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.Str));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.Date));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.StrLen));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.TrimL));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.TrimR));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.TrimLR));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.Left));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.Right));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.Mid));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.StrPos));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.UCase));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.LCase));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.Chr));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.ChrCode));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.EmptyStr));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.StrReplace));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.Year));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.Month));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.Day));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.Hour));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.Minute));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.Second));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.BegOfYear));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.BegOfMonth));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.BegOfDay));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.BegOfHour));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.BegOfMinute));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.BegOfQuarter));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.EndOfYear));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.EndOfMonth));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.EndOfDay));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.EndOfHour));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.EndOfMinute));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.EndOfQuarter));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.WeekOfYear));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.DayOfYear));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.DayOfWeek));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.AddMonth));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.CurrentDate));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.Integer));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.Round));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.Pow));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.Sqrt));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.ExceptionInfo));
            Assert.True(LanguageDef.IsBuiltInFunction(Token.ExceptionDescr));
        }

        [Fact]
        public void All_Tokens()
        {
            Assert.Equal(Token.If, LanguageDef.GetToken("если"));
            Assert.Equal(Token.Then, LanguageDef.GetToken("тогда"));
            Assert.Equal(Token.Else, LanguageDef.GetToken("иначе"));
            Assert.Equal(Token.ElseIf, LanguageDef.GetToken("иначеесли"));
            Assert.Equal(Token.EndIf, LanguageDef.GetToken("конецесли"));
            Assert.Equal(Token.VarDef, LanguageDef.GetToken("перем"));
            Assert.Equal(Token.ByValParam, LanguageDef.GetToken("знач"));
            Assert.Equal(Token.Procedure, LanguageDef.GetToken("процедура"));
            Assert.Equal(Token.EndProcedure, LanguageDef.GetToken("конецпроцедуры"));
            Assert.Equal(Token.Function, LanguageDef.GetToken("функция"));
            Assert.Equal(Token.EndFunction, LanguageDef.GetToken("конецфункции"));
            Assert.Equal(Token.For, LanguageDef.GetToken("для"));
            Assert.Equal(Token.Each, LanguageDef.GetToken("каждого"));
            Assert.Equal(Token.In, LanguageDef.GetToken("из"));
            Assert.Equal(Token.To, LanguageDef.GetToken("по"));
            Assert.Equal(Token.While, LanguageDef.GetToken("пока"));
            Assert.Equal(Token.Loop, LanguageDef.GetToken("цикл"));
            Assert.Equal(Token.EndLoop, LanguageDef.GetToken("конеццикла"));
            Assert.Equal(Token.Return, LanguageDef.GetToken("возврат"));
            Assert.Equal(Token.Continue, LanguageDef.GetToken("продолжить"));
            Assert.Equal(Token.Break, LanguageDef.GetToken("прервать"));
            Assert.Equal(Token.Try, LanguageDef.GetToken("попытка"));
            Assert.Equal(Token.Exception, LanguageDef.GetToken("исключение"));
            Assert.Equal(Token.RaiseException, LanguageDef.GetToken("вызватьисключение"));
            Assert.Equal(Token.EndTry, LanguageDef.GetToken("конецпопытки"));
            Assert.Equal(Token.NewObject, LanguageDef.GetToken("новый"));
            Assert.Equal(Token.Export, LanguageDef.GetToken("экспорт"));

            Assert.Equal(Token.If, LanguageDef.GetToken("if"));
            Assert.Equal(Token.Then, LanguageDef.GetToken("then"));
            Assert.Equal(Token.Else, LanguageDef.GetToken("else"));
            Assert.Equal(Token.ElseIf, LanguageDef.GetToken("elsif"));
            Assert.Equal(Token.EndIf, LanguageDef.GetToken("endif"));
            Assert.Equal(Token.VarDef, LanguageDef.GetToken("var"));
            Assert.Equal(Token.ByValParam, LanguageDef.GetToken("val"));
            Assert.Equal(Token.Procedure, LanguageDef.GetToken("procedure"));
            Assert.Equal(Token.EndProcedure, LanguageDef.GetToken("endprocedure"));
            Assert.Equal(Token.Function, LanguageDef.GetToken("function"));
            Assert.Equal(Token.EndFunction, LanguageDef.GetToken("endfunction"));
            Assert.Equal(Token.For, LanguageDef.GetToken("for"));
            Assert.Equal(Token.Each, LanguageDef.GetToken("each"));
            Assert.Equal(Token.In, LanguageDef.GetToken("in"));
            Assert.Equal(Token.To, LanguageDef.GetToken("to"));
            Assert.Equal(Token.While, LanguageDef.GetToken("while"));
            Assert.Equal(Token.Loop, LanguageDef.GetToken("do"));
            Assert.Equal(Token.EndLoop, LanguageDef.GetToken("enddo"));
            Assert.Equal(Token.Return, LanguageDef.GetToken("return"));
            Assert.Equal(Token.Continue, LanguageDef.GetToken("continue"));
            Assert.Equal(Token.Break, LanguageDef.GetToken("break"));
            Assert.Equal(Token.Try, LanguageDef.GetToken("try"));
            Assert.Equal(Token.Exception, LanguageDef.GetToken("except"));
            Assert.Equal(Token.RaiseException, LanguageDef.GetToken("raise"));
            Assert.Equal(Token.EndTry, LanguageDef.GetToken("endtry"));
            Assert.Equal(Token.NewObject, LanguageDef.GetToken("new"));
            Assert.Equal(Token.Export, LanguageDef.GetToken("export"));

            Assert.Equal(Token.Plus, LanguageDef.GetToken("+"));
            Assert.Equal(Token.Minus, LanguageDef.GetToken("-"));
            Assert.Equal(Token.Multiply, LanguageDef.GetToken("*"));
            Assert.Equal(Token.Division, LanguageDef.GetToken("/"));
            Assert.Equal(Token.LessThan, LanguageDef.GetToken("<"));
            Assert.Equal(Token.LessOrEqual, LanguageDef.GetToken("<="));
            Assert.Equal(Token.MoreThan, LanguageDef.GetToken(">"));
            Assert.Equal(Token.MoreOrEqual, LanguageDef.GetToken(">="));
            Assert.Equal(Token.NotEqual, LanguageDef.GetToken("<>"));
            Assert.Equal(Token.Modulo, LanguageDef.GetToken("%"));
            Assert.Equal(Token.And, LanguageDef.GetToken("и"));
            Assert.Equal(Token.Or, LanguageDef.GetToken("или"));
            Assert.Equal(Token.Not, LanguageDef.GetToken("не"));
            Assert.Equal(Token.OpenPar, LanguageDef.GetToken("("));
            Assert.Equal(Token.ClosePar, LanguageDef.GetToken(")"));
            Assert.Equal(Token.OpenBracket, LanguageDef.GetToken("["));
            Assert.Equal(Token.CloseBracket, LanguageDef.GetToken("]"));
            Assert.Equal(Token.Dot, LanguageDef.GetToken("."));
            Assert.Equal(Token.Comma, LanguageDef.GetToken(","));
            Assert.Equal(Token.Equal, LanguageDef.GetToken("="));
            Assert.Equal(Token.Semicolon, LanguageDef.GetToken(";"));

            Assert.Equal(Token.Question, LanguageDef.GetToken("?"));
            Assert.Equal(Token.Bool, LanguageDef.GetToken("булево"));
            Assert.Equal(Token.Number, LanguageDef.GetToken("число"));
            Assert.Equal(Token.Str, LanguageDef.GetToken("строка"));
            Assert.Equal(Token.Date, LanguageDef.GetToken("дата"));

            Assert.Equal(Token.StrLen, LanguageDef.GetToken("стрдлина"));
            Assert.Equal(Token.TrimL, LanguageDef.GetToken("сокрл"));
            Assert.Equal(Token.TrimR, LanguageDef.GetToken("сокрп"));
            Assert.Equal(Token.TrimLR, LanguageDef.GetToken("сокрлп"));
            Assert.Equal(Token.Left, LanguageDef.GetToken("лев"));
            Assert.Equal(Token.Right, LanguageDef.GetToken("прав"));
            Assert.Equal(Token.Mid, LanguageDef.GetToken("сред"));
            Assert.Equal(Token.StrPos, LanguageDef.GetToken("найти"));
            Assert.Equal(Token.UCase, LanguageDef.GetToken("врег"));
            Assert.Equal(Token.LCase, LanguageDef.GetToken("нрег"));
            Assert.Equal(Token.Chr, LanguageDef.GetToken("символ"));
            Assert.Equal(Token.ChrCode, LanguageDef.GetToken("кодсимвола"));
            Assert.Equal(Token.EmptyStr, LanguageDef.GetToken("пустаястрока"));
            Assert.Equal(Token.StrReplace, LanguageDef.GetToken("стрзаменить"));

            Assert.Equal(Token.Year, LanguageDef.GetToken("год"));
            Assert.Equal(Token.Month, LanguageDef.GetToken("месяц"));
            Assert.Equal(Token.Day, LanguageDef.GetToken("день"));
            Assert.Equal(Token.Hour, LanguageDef.GetToken("час"));
            Assert.Equal(Token.Minute, LanguageDef.GetToken("минута"));
            Assert.Equal(Token.Second, LanguageDef.GetToken("секунда"));
            Assert.Equal(Token.BegOfYear, LanguageDef.GetToken("началогода"));
            Assert.Equal(Token.BegOfMonth, LanguageDef.GetToken("началомесяца"));
            Assert.Equal(Token.BegOfDay, LanguageDef.GetToken("началодня"));
            Assert.Equal(Token.BegOfHour, LanguageDef.GetToken("началочаса"));
            Assert.Equal(Token.BegOfMinute, LanguageDef.GetToken("началоминуты"));
            Assert.Equal(Token.BegOfQuarter, LanguageDef.GetToken("началоквартала"));
            Assert.Equal(Token.EndOfYear, LanguageDef.GetToken("конецгода"));
            Assert.Equal(Token.EndOfMonth, LanguageDef.GetToken("конецмесяца"));
            Assert.Equal(Token.EndOfDay, LanguageDef.GetToken("конецдня"));
            Assert.Equal(Token.EndOfHour, LanguageDef.GetToken("конецчаса"));
            Assert.Equal(Token.EndOfMinute, LanguageDef.GetToken("конецминуты"));
            Assert.Equal(Token.EndOfQuarter, LanguageDef.GetToken("конецквартала"));
            Assert.Equal(Token.WeekOfYear, LanguageDef.GetToken("неделягода"));
            Assert.Equal(Token.DayOfYear, LanguageDef.GetToken("деньгода"));
            Assert.Equal(Token.DayOfWeek, LanguageDef.GetToken("деньнедели"));
            Assert.Equal(Token.AddMonth, LanguageDef.GetToken("добавитьмесяц"));
            Assert.Equal(Token.CurrentDate, LanguageDef.GetToken("текущаядата"));
            Assert.Equal(Token.Integer, LanguageDef.GetToken("цел"));
            Assert.Equal(Token.Round, LanguageDef.GetToken("окр"));
            Assert.Equal(Token.Pow, LanguageDef.GetToken("pow"));
            Assert.Equal(Token.Sqrt, LanguageDef.GetToken("sqrt"));
            Assert.Equal(Token.ExceptionInfo, LanguageDef.GetToken("информацияобошибке"));
            Assert.Equal(Token.ExceptionDescr, LanguageDef.GetToken("описаниеошибки"));
        }

        [Fact]
        public void Keywords_Which_Begins_New_Statement()
        {
            Assert.True(LanguageDef.IsBeginOfStatement(Token.VarDef));
            Assert.True(LanguageDef.IsBeginOfStatement(Token.If));
            Assert.True(LanguageDef.IsBeginOfStatement(Token.For));
            Assert.True(LanguageDef.IsBeginOfStatement(Token.While));
            Assert.True(LanguageDef.IsBeginOfStatement(Token.Try));
        }

        [Fact]
        public void IsValidIdentifier()
        {
            Assert.True(LanguageDef.IsValidIdentifier("Var"));
            Assert.True(LanguageDef.IsValidIdentifier("Var123"));
            Assert.True(LanguageDef.IsValidIdentifier("Var_123"));
            Assert.True(LanguageDef.IsValidIdentifier("_Var"));
            Assert.False(LanguageDef.IsValidIdentifier("123Var"));
            Assert.False(LanguageDef.IsValidIdentifier("V a r"));
            Assert.False(LanguageDef.IsValidIdentifier("Var$"));
            Assert.False(LanguageDef.IsValidIdentifier(null));

        }
    }
}
