using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    public static class LanguageDef
    {
        static Dictionary<Token, int> _priority = new Dictionary<Token, int>();
        static Dictionary<string, Token> _stringToToken = new Dictionary<string, Token>(StringComparer.InvariantCultureIgnoreCase);

        static LanguageDef()
        {
            _priority.Add(Token.Plus, 5);
            _priority.Add(Token.Minus, 5);
            _priority.Add(Token.UnaryMinus, 5);
            _priority.Add(Token.Multiply, 6);
            _priority.Add(Token.Division, 6);
            _priority.Add(Token.Modulo, 6);

            _priority.Add(Token.Or, 1);
            _priority.Add(Token.And, 2);
            _priority.Add(Token.Not, 3);

            _priority.Add(Token.Equal, 4);
            _priority.Add(Token.MoreThan, 4);
            _priority.Add(Token.LessThan, 4);
            _priority.Add(Token.MoreOrEqual, 4);
            _priority.Add(Token.LessOrEqual, 4);
            _priority.Add(Token.NotEqual, 4);

            // tokens
            _stringToToken.Add("если", Token.If);
            _stringToToken.Add("тогда", Token.Then);
            _stringToToken.Add("иначе", Token.Else);
            _stringToToken.Add("иначеесли", Token.ElseIf);
            _stringToToken.Add("конецесли", Token.EndIf);
            _stringToToken.Add("перем", Token.VarDef);
            _stringToToken.Add("знач", Token.ByValParam);
            _stringToToken.Add("процедура", Token.Procedure);
            _stringToToken.Add("конецпроцедуры", Token.EndProcedure);
            _stringToToken.Add("функция", Token.Function);
            _stringToToken.Add("конецфункции", Token.EndFunction);
            _stringToToken.Add("для", Token.For);
            _stringToToken.Add("каждого", Token.Each);
            _stringToToken.Add("из", Token.In);
            _stringToToken.Add("по", Token.To);
            _stringToToken.Add("пока", Token.While);
            _stringToToken.Add("цикл", Token.Loop);
            _stringToToken.Add("конеццикла", Token.EndLoop);
            _stringToToken.Add("возврат", Token.Return);
            _stringToToken.Add("продолжить", Token.Continue);
            _stringToToken.Add("прервать", Token.Break);
            _stringToToken.Add("попытка", Token.Try);
            _stringToToken.Add("исключение", Token.Exception);
            _stringToToken.Add("вызватьисключение", Token.RaiseException);
            _stringToToken.Add("конецпопытки", Token.EndTry);
            _stringToToken.Add("новый", Token.NewObject);
            _stringToToken.Add("экспорт", Token.Export);

            _stringToToken.Add("if", Token.If);
            _stringToToken.Add("then", Token.Then);
            _stringToToken.Add("else", Token.Else);
            _stringToToken.Add("elseif", Token.ElseIf);
            _stringToToken.Add("endif", Token.EndIf);
            _stringToToken.Add("var", Token.VarDef);
            _stringToToken.Add("val", Token.ByValParam);
            _stringToToken.Add("procedure", Token.Procedure);
            _stringToToken.Add("endprocedure", Token.EndProcedure);
            _stringToToken.Add("function", Token.Function);
            _stringToToken.Add("endfunction", Token.EndFunction);
            _stringToToken.Add("for", Token.For);
            _stringToToken.Add("each", Token.Each);
            _stringToToken.Add("from", Token.In);
            _stringToToken.Add("to", Token.To);
            _stringToToken.Add("while", Token.While);
            _stringToToken.Add("do", Token.Loop);
            _stringToToken.Add("enddo", Token.EndLoop);
            _stringToToken.Add("return", Token.Return);
            _stringToToken.Add("contınue", Token.Continue);
            _stringToToken.Add("break", Token.Break);
            _stringToToken.Add("try", Token.Try);
            _stringToToken.Add("exception", Token.Exception);
            _stringToToken.Add("raise", Token.RaiseException);
            _stringToToken.Add("endtry", Token.EndTry);
            _stringToToken.Add("new", Token.NewObject);
            _stringToToken.Add("export", Token.Export);

            _stringToToken.Add("+", Token.Plus);
            _stringToToken.Add("-", Token.Minus);
            _stringToToken.Add("*", Token.Multiply);
            _stringToToken.Add("/", Token.Division);
            _stringToToken.Add("<", Token.LessThan);
            _stringToToken.Add("<=", Token.LessOrEqual);
            _stringToToken.Add(">", Token.MoreThan);
            _stringToToken.Add(">=", Token.MoreOrEqual);
            _stringToToken.Add("<>", Token.NotEqual);
            _stringToToken.Add("%", Token.Modulo);
            _stringToToken.Add("и", Token.And);
            _stringToToken.Add("или", Token.Or);
            _stringToToken.Add("не", Token.Not);
            _stringToToken.Add("(", Token.OpenPar);
            _stringToToken.Add(")", Token.ClosePar);
            _stringToToken.Add("[", Token.OpenBracket);
            _stringToToken.Add("]", Token.CloseBracket);
            _stringToToken.Add(".", Token.Dot);
            _stringToToken.Add(",", Token.Comma);
            _stringToToken.Add("=", Token.Equal);
            _stringToToken.Add(";", Token.Semicolon);

            _stringToToken.Add("?", Token.Question);
            _stringToToken.Add("булево", Token.Bool);
            _stringToToken.Add("число", Token.Number);
            _stringToToken.Add("строка", Token.Str);
            _stringToToken.Add("дата", Token.Date);

            _stringToToken.Add("стрдлина", Token.StrLen);
            _stringToToken.Add("сокрл", Token.TrimL);
            _stringToToken.Add("сокрп", Token.TrimR);
            _stringToToken.Add("сокрлп", Token.TrimLR);
            _stringToToken.Add("лев", Token.Left);
            _stringToToken.Add("прав", Token.Right);
            _stringToToken.Add("сред", Token.Mid);
            _stringToToken.Add("найти", Token.StrPos);
            _stringToToken.Add("врег", Token.UCase);
            _stringToToken.Add("нрег", Token.LCase);
            _stringToToken.Add("символ", Token.Chr);
            _stringToToken.Add("кодсимвола", Token.ChrCode);
            _stringToToken.Add("пустаястрока", Token.EmptyStr);
            _stringToToken.Add("стрзаменить", Token.StrReplace);

            _stringToToken.Add("год", Token.Year);
            _stringToToken.Add("месяц", Token.Month);
            _stringToToken.Add("день", Token.Day);
            _stringToToken.Add("час", Token.Hour);
            _stringToToken.Add("минута", Token.Minute);
            _stringToToken.Add("секунда", Token.Second);
            _stringToToken.Add("началогода", Token.BegOfYear);
            _stringToToken.Add("началомесяца", Token.BegOfMonth);
            _stringToToken.Add("началодня", Token.BegOfDay);
            _stringToToken.Add("началочаса", Token.BegOfHour);
            _stringToToken.Add("началоминуты", Token.BegOfMinute);
            _stringToToken.Add("началоквартала", Token.BegOfQuarter);
            _stringToToken.Add("конецгода", Token.EndOfYear);
            _stringToToken.Add("конецмесяца", Token.EndOfMonth);
            _stringToToken.Add("конецдня", Token.EndOfDay);
            _stringToToken.Add("конецчаса", Token.EndOfHour);
            _stringToToken.Add("конецминуты", Token.EndOfMinute);
            _stringToToken.Add("конецквартала", Token.EndOfQuarter);
            _stringToToken.Add("неделягода", Token.WeekOfYear);
            _stringToToken.Add("деньгода", Token.DayOfYear);
            _stringToToken.Add("деньнедели", Token.DayOfWeek);
            _stringToToken.Add("добавитьмесяц", Token.AddMonth);
            _stringToToken.Add("текущаядата", Token.CurrentDate);
            _stringToToken.Add("цел", Token.Integer);
            _stringToToken.Add("окр", Token.Round);
            _stringToToken.Add("pow", Token.Pow);
            _stringToToken.Add("sqrt", Token.Sqrt);
            _stringToToken.Add("информацияобошибке", Token.ExceptionInfo);
            _stringToToken.Add("описаниеошибки", Token.ExceptionDescr);

        }

        public static Token GetToken(string tokText)
        {
            Token result;
            if (_stringToToken.TryGetValue(tokText, out result))
            {
                return result;
            }
            else
            {
                return Token.NotAToken;
            }
        }

        public static int GetPriority(Token op)
        {
            return _priority[op];
        }

        public static bool IsBuiltInFunction(Token token)
        {
            const int BUILTINS_INDEX = (int)Token.ByValParam;
            return (int)token > BUILTINS_INDEX;
        }

        public static bool IsBeginOfStatement(Token token)
        {
            switch(token)
            {
                case Token.VarDef:
                case Token.If:
                case Token.For:
                case Token.While:
                case Token.Try:
                    return true;
                default:
                    return false;
            }
        }
    }

    public static class SpecialChars
    {
        public const char StringQuote = '"';
        public const char DateQuote = '\'';
        public const char EndOperator = ';';

        public static bool IsOperatorChar(char symbol)
        {
            switch (symbol)
            {
                case '+':
                case '-':
                case '*':
                case '/':
                case '<':
                case '>':
                case '=':
                case '%':
                case '(':
                case ')':
                case '.':
                case ',':
                case '[':
                case ']':
                    return true;
                default:
                    return false;

            }
        }

        public static bool IsDelimiter(char symbol)
        {
            return !(Char.IsLetterOrDigit(symbol) || symbol == '_');
        }

    }
}
