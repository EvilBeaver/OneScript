﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Runtime.CompilerServices;
using OneScript.Language;
using OneScript.Localization;

namespace ScriptEngine.Compiler
{
    public static class CompilerErrors
    {
        public static CodeError UseProcAsFunction() =>
            Create("Использование процедуры, как функции", "Procedure called as function");
        
        public static CodeError TooFewArgumentsPassed() =>
            Create("Недостаточно фактических параметров", "Too many actual parameters");

        public static CodeError TooManyArgumentsPassed() =>
            Create("Слишком много фактических параметров", "Too many actual parameters");

        public static CodeError MissedArgument() =>
            Create("Пропущен обязательный параметр", "Missed mandatory parameter");

        private static CodeError Create(string ru, string en, [CallerMemberName] string errorId = default)
        {
            return new CodeError
            {
                ErrorId = errorId,
                Description = BilingualString.Localize(ru, en)
            };
        }
    }
}