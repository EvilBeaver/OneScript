/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language
{
    public static class LanguageDef
    {
        static readonly Dictionary<Token, int> _priority = new Dictionary<Token, int>();
        public const int MAX_OPERATION_PRIORITY = 8;

        private static readonly LexemTrie<Token> _stringToToken = new LexemTrie<Token>();

        private static readonly LexemTrie<bool> _undefined = new LexemTrie<bool>();
        private static readonly LexemTrie<bool> _booleans = new LexemTrie<bool>();
        private static readonly LexemTrie<bool> _logicalOp = new LexemTrie<bool>();

        private static readonly LexemTrie<bool> _preprocRegion = new LexemTrie<bool>();
        private static readonly LexemTrie<bool> _preprocEndRegion = new LexemTrie<bool>();
        
        private static readonly LexemTrie<bool> _preprocImport = new LexemTrie<bool>();

        const int BUILTINS_INDEX = (int)Token.ByValParam;

        static LanguageDef()
        {
            _priority.Add(Token.Plus, 5);
            _priority.Add(Token.Minus, 5);
            _priority.Add(Token.Multiply, 6);
            _priority.Add(Token.Division, 6);
            _priority.Add(Token.Modulo, 6);
            _priority.Add(Token.UnaryPlus, 7);
            _priority.Add(Token.UnaryMinus, 7);

            _priority.Add(Token.Or, 1);
            _priority.Add(Token.And, 2);
            _priority.Add(Token.Not, 3);

            _priority.Add(Token.Equal, 4);
            _priority.Add(Token.MoreThan, 4);
            _priority.Add(Token.LessThan, 4);
            _priority.Add(Token.MoreOrEqual, 4);
            _priority.Add(Token.LessOrEqual, 4);
            _priority.Add(Token.NotEqual, 4);

            #region constants

            _undefined.Add("Undefined", true);
            _undefined.Add("Неопределено", true);

            _booleans.Add("True", true);
            _booleans.Add("False", true);
            _booleans.Add("Истина", true);
            _booleans.Add("Ложь", true);

            _logicalOp.Add("And", true);
            _logicalOp.Add("Or", true);
            _logicalOp.Add("Not", true);

            _logicalOp.Add("И", true);
            _logicalOp.Add("ИЛИ", true);
            _logicalOp.Add("НЕ", true);

            #endregion

            // tokens

            #region Ключевые слова

            AddToken(Token.If, "если", "if");
            AddToken(Token.Then, "тогда", "then");
            AddToken(Token.Else, "иначе", "else");
            AddToken(Token.ElseIf, "иначеесли", "elsif");
            AddToken(Token.EndIf, "конецесли", "endif");
            AddToken(Token.VarDef, "перем", "var");
            AddToken(Token.ByValParam, "знач", "val");
            AddToken(Token.Procedure, "процедура", "procedure");
            AddToken(Token.EndProcedure, "конецпроцедуры", "endprocedure");
            AddToken(Token.Function, "функция", "function");
            AddToken(Token.EndFunction, "конецфункции", "endfunction");
            AddToken(Token.For, "для", "for");
            AddToken(Token.Each, "каждого", "each");
            AddToken(Token.In, "из", "in");
            AddToken(Token.To, "по", "to");
            AddToken(Token.While, "пока", "while");
            AddToken(Token.Loop, "цикл", "do");
            AddToken(Token.EndLoop, "конеццикла", "enddo");
            AddToken(Token.Return, "возврат", "return");
            AddToken(Token.Continue, "продолжить", "continue");
            AddToken(Token.Break, "прервать", "break");
            AddToken(Token.Try, "попытка", "try");
            AddToken(Token.Exception, "исключение", "except");
            AddToken(Token.Execute, "выполнить", "execute");
            AddToken(Token.RaiseException, "вызватьисключение", "raise");
            AddToken(Token.EndTry, "конецпопытки", "endtry");
            AddToken(Token.NewObject, "новый", "new");
            AddToken(Token.Export, "экспорт", "export");
            AddToken(Token.And, "и", "and");
            AddToken(Token.Or, "или", "or");
            AddToken(Token.Not, "не", "not");
            AddToken(Token.AddHandler, "ДобавитьОбработчик", "AddHandler");
            AddToken(Token.RemoveHandler, "УдалитьОбработчик", "RemoveHandler");

            #endregion

            #region Операторы

            AddToken(Token.Plus, "+");
            AddToken(Token.Minus, "-");
            AddToken(Token.Multiply, "*");
            AddToken(Token.Division, "/");
            AddToken(Token.LessThan, "<");
            AddToken(Token.LessOrEqual, "<=");
            AddToken(Token.MoreThan, ">");
            AddToken(Token.MoreOrEqual, ">=");
            AddToken(Token.NotEqual, "<>");
            AddToken(Token.Modulo, "%");
            AddToken(Token.OpenPar, "(");
            AddToken(Token.ClosePar, ")");
            AddToken(Token.OpenBracket, "[");
            AddToken(Token.CloseBracket, "]");
            AddToken(Token.Dot, ".");
            AddToken(Token.Comma, ",");
            AddToken(Token.Equal, "=");
            AddToken(Token.Semicolon, ";");
            AddToken(Token.Question, "?");

            #endregion

            #region Функции работы с типами

            AddToken(Token.Bool, "булево", "boolean");
            AddToken(Token.Number, "число", "number");
            AddToken(Token.Str, "строка", "string");
            AddToken(Token.Date, "дата", "date");
            AddToken(Token.Type, "тип", "type");
            AddToken(Token.ValType, "типзнч", "typeof");

            #endregion

            #region Встроенные функции

            AddToken(Token.Eval, "вычислить", "eval");
            AddToken(Token.StrLen, "стрдлина", "strlen");
            AddToken(Token.TrimL, "сокрл", "triml");
            AddToken(Token.TrimR, "сокрп", "trimr");
            AddToken(Token.TrimLR, "сокрлп", "trimall");
            AddToken(Token.Left, "лев", "left");
            AddToken(Token.Right, "прав", "right");
            AddToken(Token.Mid, "сред", "mid");
            AddToken(Token.StrPos, "найти", "find");
            AddToken(Token.UCase, "врег", "upper");
            AddToken(Token.LCase, "нрег", "lower");
            AddToken(Token.TCase, "трег", "title");
            AddToken(Token.Chr, "символ", "char");
            AddToken(Token.ChrCode, "кодсимвола", "charcode");
            AddToken(Token.EmptyStr, "пустаястрока", "isblankstring");
            AddToken(Token.StrReplace, "стрзаменить", "strreplace");
            AddToken(Token.StrGetLine, "стрполучитьстроку", "strgetline");
            AddToken(Token.StrLineCount, "стрчислострок", "strlinecount");
            AddToken(Token.StrEntryCount, "стрчисловхождений", "stroccurrencecount");
            AddToken(Token.Year, "год", "year");
            AddToken(Token.Month, "месяц", "month");
            AddToken(Token.Day, "день", "day");
            AddToken(Token.Hour, "час", "hour");
            AddToken(Token.Minute, "минута", "minute");
            AddToken(Token.Second, "секунда", "second");
            AddToken(Token.BegOfYear, "началогода", "begofyear");
            AddToken(Token.BegOfMonth, "началомесяца", "begofmonth");
            AddToken(Token.BegOfDay, "началодня", "begofday");
            AddToken(Token.BegOfHour, "началочаса", "begofhour");
            AddToken(Token.BegOfMinute, "началоминуты", "begofminute");
            AddToken(Token.BegOfQuarter, "началоквартала", "begofquarter");
            AddToken(Token.EndOfYear, "конецгода", "endofyear");
            AddToken(Token.EndOfMonth, "конецмесяца", "endofmonth");
            AddToken(Token.EndOfDay, "конецдня", "endofday");
            AddToken(Token.EndOfHour, "конецчаса", "endofhour");
            AddToken(Token.EndOfMinute, "конецминуты", "endofminute");
            AddToken(Token.EndOfQuarter, "конецквартала", "endofquarter");
            AddToken(Token.WeekOfYear, "неделягода", "weekofyear");
            AddToken(Token.DayOfYear, "деньгода", "dayofyear");
            AddToken(Token.DayOfWeek, "деньнедели", "dayofweek");
            AddToken(Token.AddMonth, "добавитьмесяц", "addmonth");
            AddToken(Token.CurrentDate, "текущаядата", "currentdate");
            AddToken(Token.Integer, "цел", "int");
            AddToken(Token.Round, "окр", "round");
            AddToken(Token.Log, "log");
            AddToken(Token.Log10, "log10");
            AddToken(Token.Sin, "sin");
            AddToken(Token.Cos, "cos");
            AddToken(Token.Tan, "tan");
            AddToken(Token.ASin, "asin");
            AddToken(Token.ACos, "acos");
            AddToken(Token.ATan, "atan");
            AddToken(Token.Exp, "exp");
            AddToken(Token.Pow, "pow");
            AddToken(Token.Sqrt, "sqrt");
            AddToken(Token.Min, "мин", "min");
            AddToken(Token.Max, "макс", "max");
            AddToken(Token.Format, "формат", "format");
            AddToken(Token.ExceptionInfo, "информацияобошибке", "errorinfo");
            AddToken(Token.ExceptionDescr, "описаниеошибки", "errordescription");
            AddToken(Token.ModuleInfo, "текущийсценарий", "currentscript");

            #endregion

            _preprocRegion.Add("Область",true);
            _preprocRegion.Add("Region", true);
            _preprocEndRegion.Add("КонецОбласти", true);
            _preprocEndRegion.Add("EndRegion", true);

            _preprocImport.Add("Использовать", true);
            _preprocImport.Add("Use", true);
        }

        private static void AddToken(Token token, string name)
        {
            _stringToToken.Add(name, token);
        }

        private static void AddToken(Token token, string name, string alias)
        {
            _stringToToken.Add(name, token);
            _stringToToken.Add(alias, token);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBuiltInFunction(Token token)
        {
            return (int)token > BUILTINS_INDEX;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBinaryOperator(Token token)
        {
            return token == Token.Plus
                   || token == Token.Minus
                   || token == Token.Multiply
                   || token == Token.Division
                   || token == Token.Modulo
                   || token == Token.And
                   || token == Token.Or
                   || token == Token.Not
                   || token == Token.LessThan
                   || token == Token.LessOrEqual
                   || token == Token.MoreThan
                   || token == Token.MoreOrEqual
                   || token == Token.Equal
                   || token == Token.NotEqual;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLogicalBinaryOperator(Token token)
        {
            return token == Token.And || token == Token.Or;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUnaryOperator(Token token)
        {
            return token == Token.Plus || token == Token.Minus || token == Token.Not;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLiteral(ref Lexem lex)
        {
            return lex.Type == LexemType.StringLiteral
                   || lex.Type == LexemType.NumberLiteral
                   || lex.Type == LexemType.BooleanLiteral
                   || lex.Type == LexemType.DateLiteral
                   || lex.Type == LexemType.UndefinedLiteral
                   || lex.Type == LexemType.NullLiteral;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidPropertyName(in Lexem lex)
        {
            return lex.Type == LexemType.Identifier 
                   || lex.Type == LexemType.BooleanLiteral
                   || lex.Type == LexemType.NullLiteral
                   || lex.Type == LexemType.UndefinedLiteral
                   || lex.Token == Token.And
                   || lex.Token == Token.Or
                   || lex.Token == Token.Not;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUserSymbol(in Lexem lex)
        {
            return lex.Type == LexemType.Identifier && lex.Token == Token.NotAToken;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsIdentifier(ref Lexem lex)
        {
            return lex.Type == LexemType.Identifier;
        }

        public static bool IsValidIdentifier(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            if (!(char.IsLetter(name[0]) || name[0] == SpecialChars.Underscore))
                return false;

            for (int i = 1; i < name.Length; i++)
            {
                if (!(char.IsLetterOrDigit(name[i]) || name[i] == SpecialChars.Underscore))
                    return false;
            }

            return true;
        }

        public static Token[] BuiltInFunctions()
        {
            var values = Enum.GetValues(typeof(Token));
            var result = new Token[values.Length - BUILTINS_INDEX - 1];
            for (int i = BUILTINS_INDEX + 1, j = 0; i < values.Length; i++, j++)
            {
                result[j] = (Token)values.GetValue(i);
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBeginOfStatement(Token token)
        {
            switch (token)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEndOfBlockToken(Token token)
        {
            return token == Token.EndIf
                   || token == Token.EndProcedure
                   || token == Token.EndFunction
                   || token == Token.Else
                   || token == Token.EndLoop
                   || token == Token.EndTry
                   || token == Token.EndOfText
                   || token == Token.ElseIf;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBooleanLiteralString(string value)
        {
            return _booleans.TryGetValue(value, out var nodeIsFilled) && nodeIsFilled;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUndefinedString(string value)
        {
            return _undefined.TryGetValue(value, out var nodeIsFilled) && nodeIsFilled;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullString(string value)
        {
            return string.Compare(value, "Null", StringComparison.OrdinalIgnoreCase) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLogicalOperatorString(string content)
        {
            return _logicalOp.TryGetValue(content, out var nodeIsFilled) && nodeIsFilled;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPreprocRegion(string value)
        {
            return _preprocRegion.TryGetValue(value, out var nodeIsFilled) && nodeIsFilled;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPreprocEndRegion(string value)
        {
            return _preprocEndRegion.TryGetValue(value, out var nodeIsFilled) && nodeIsFilled;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsImportDirective(string value)
        {
            return _preprocImport.TryGetValue(value, out var nodeIsFilled) && nodeIsFilled;
        }
    }
}
