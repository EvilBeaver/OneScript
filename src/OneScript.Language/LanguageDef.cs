/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace OneScript.Language
{
    public static class LanguageDef
    {
        static readonly Dictionary<Token, int> _priority = new Dictionary<Token, int>();
        public const int MAX_OPERATION_PRIORITY = 8;

        private static readonly Dictionary<Token, (string, string)> _keywords = new Dictionary<Token, (string, string)>();
        
        private static readonly IdentifiersTrie<Token> _stringToToken = new IdentifiersTrie<Token>();

        const int BUILTINS_INDEX = (int)Token.ByValParam;

        public enum WordType
        {
            Undefined,
            Boolean,
            Logical,
            Null,
            Preproc,
            None
        };

        private static readonly IdentifiersTrie<WordType> _specwords = new IdentifiersTrie<WordType>();

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

            _specwords.Add("Undefined", WordType.Undefined);
            _specwords.Add("Неопределено", WordType.Undefined);

            _specwords.Add("True", WordType.Boolean);
            _specwords.Add("False", WordType.Boolean);
            _specwords.Add("Истина", WordType.Boolean);
            _specwords.Add("Ложь", WordType.Boolean);

            _specwords.Add("And", WordType.Logical);
            _specwords.Add("Or", WordType.Logical);
            _specwords.Add("Not", WordType.Logical);

            _specwords.Add("И", WordType.Logical);
            _specwords.Add("ИЛИ", WordType.Logical);
            _specwords.Add("НЕ", WordType.Logical);

            _specwords.Add("NULL", WordType.Null);

            _specwords.Add("Использовать", WordType.Preproc);
            _specwords.Add("Use", WordType.Preproc);

            #endregion

            // tokens

            #region Ключевые слова

            AddKeyword(Token.If, "Если", "If");
            AddKeyword(Token.Then, "Тогда", "Then");
            AddKeyword(Token.Else, "Иначе", "Else");
            AddKeyword(Token.ElseIf, "ИначеЕсли", "ElsIf");
            AddKeyword(Token.EndIf, "КонецЕсли", "EndIf");
            AddKeyword(Token.VarDef, "Перем", "Var");
            AddKeyword(Token.ByValParam, "Знач", "Val");
            AddKeyword(Token.Procedure, "Процедура", "Procedure");
            AddKeyword(Token.EndProcedure, "КонецПроцедуры", "EndProcedure");
            AddKeyword(Token.Function, "Функция", "Function");
            AddKeyword(Token.EndFunction, "КонецФункции", "EndFunction");
            AddKeyword(Token.For, "Для", "For");
            AddKeyword(Token.Each, "Каждого", "Each");
            AddKeyword(Token.In, "Из", "In");
            AddKeyword(Token.To, "По", "To");
            AddKeyword(Token.While, "Пока", "While");
            AddKeyword(Token.Loop, "Цикл", "Do");
            AddKeyword(Token.EndLoop, "КонецЦикла", "EndDo");
            AddKeyword(Token.Return, "Возврат", "Return");
            AddKeyword(Token.Continue, "Продолжить", "Continue");
            AddKeyword(Token.Break, "Прервать", "Break");
            AddKeyword(Token.Try, "Попытка", "Try");
            AddKeyword(Token.Exception, "Исключение", "Except");
            AddKeyword(Token.Execute, "Выполнить", "Execute");
            AddKeyword(Token.RaiseException, "ВызватьИсключение", "Raise");
            AddKeyword(Token.EndTry, "КонецПопытки", "EndTry");
            AddKeyword(Token.NewObject, "Новый", "New");
            AddKeyword(Token.Export, "Экспорт", "Export");
            AddKeyword(Token.And, "И", "And");
            AddKeyword(Token.Or, "Или", "Or");
            AddKeyword(Token.Not, "Не", "Not");
            AddKeyword(Token.AddHandler, "ДобавитьОбработчик", "AddHandler");
            AddKeyword(Token.RemoveHandler, "УдалитьОбработчик", "RemoveHandler");
            AddKeyword(Token.Async, "Асинх", "Async");
            AddKeyword(Token.Await, "Ждать", "Await");
            AddKeyword(Token.Goto, "Перейти", "Goto");

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
            AddToken(Token.BegOfWeek, "началонедели", "begofweek");
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
            AddToken(Token.EndOfWeek, "конецнедели", "endofweek");
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

        private static void AddKeyword(Token token, string name, string alias)
        {
            _keywords.Add(token, (name,alias));
            _stringToToken.Add(name, token);
            _stringToToken.Add(alias, token);
        }

        public static string GetTokenName(Token token)
        {
            if (_keywords.TryGetValue(token,out var strings))
            {
                return strings.Item1;
            }

            return Enum.GetName(typeof(Token), token);
        }

        public static string GetTokenAlias(Token token)
        {
            if (_keywords.TryGetValue(token,out var strings))
            {
                return strings.Item2;
            }

            return Enum.GetName(typeof(Token), token);
        }

        public static Token GetToken(string tokText)
        {
            if (_stringToToken.TryGetValue(tokText, out Token result))
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

        public static int GetBinaryPriority(Token op)
        {
            return IsBinaryOperator(op) ? _priority[op] : -1;
        }

        public static int GetUnaryPriority(Token op)
        {
            return op switch
            {
                Token.OpenPar => MAX_OPERATION_PRIORITY + 1,

                Token.Not => _priority[op],
                Token.UnaryMinus => _priority[op],
                Token.UnaryPlus => _priority[op],

                Token.Minus => _priority[Token.UnaryMinus],
                Token.Plus => _priority[Token.UnaryPlus],

                _ => MAX_OPERATION_PRIORITY
            };
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBuiltInFunction(Token token)
        {
            return (int)token > BUILTINS_INDEX;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBinaryOperator(Token token)
        {
            switch (token)
            {
                case Token.Plus:
                case Token.Minus:
                case Token.Multiply:
                case Token.Division:
                case Token.Modulo:
                case Token.Equal:
                case Token.LessThan:
                case Token.LessOrEqual:
                case Token.MoreThan:
                case Token.MoreOrEqual:
                case Token.NotEqual:
                case Token.And:
                case Token.Or:
                    return true;
                default:
                    return false;
            }
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
        public static bool IsLiteral(in Lexem lex)
        {
            switch (lex.Type)
            {
                case LexemType.StringLiteral:
                case LexemType.NumberLiteral:
                case LexemType.BooleanLiteral:
                case LexemType.DateLiteral:
                case LexemType.UndefinedLiteral:
                case LexemType.NullLiteral:
                    return true;
                default:
                    return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidPropertyName(in Lexem lex)
        {
            switch (lex.Type)
            {
                case LexemType.Identifier:
                case LexemType.BooleanLiteral:
                case LexemType.NullLiteral:
                case LexemType.UndefinedLiteral:
                    return true;

                default:
                    switch (lex.Token)
                    {
                        case Token.And:
                        case Token.Or:
                        case Token.Not:
                            return true;
                        default:
                            return false;
                    }
            }
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
            switch (token)
            {
                case Token.EndIf:
                case Token.EndProcedure:
                case Token.EndFunction:
                case Token.Else:
                case Token.EndLoop:
                case Token.EndTry:
                case Token.EndOfText:
                case Token.ElseIf:
                case Token.Exception:
                    return true;
                default:
                    return false;
            }
        }


        public static WordType GetWordType(string value)
        {
            return _specwords.TryGetValue(value, out var wordType)? wordType : WordType.None;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBooleanLiteralString(string value)
        {
            return _specwords.TryGetValue(value, out var wordType) && wordType == WordType.Boolean;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUndefinedString(string value)
        {
            return _specwords.TryGetValue(value, out var wordType) && wordType == WordType.Undefined;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullString(string value)
        {
            return string.Compare(value, "Null", StringComparison.OrdinalIgnoreCase) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLogicalOperatorString(string content)
        {
            return _specwords.TryGetValue(content, out var wordType) && wordType == WordType.Logical;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsImportDirective(string value)
        {
            return _specwords.TryGetValue(value, out var wordType) && wordType == WordType.Preproc;
        }
    }
}
