/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Runtime.CompilerServices;
using OneScript.Language;
using OneScript.Localization;

namespace OneScript.Native.Compiler
{
    public class NativeCompilerErrors
    {
        public static CodeError TypeIsNotAnObjectType(Type targetType) =>
            Create($"Тип {targetType} не является объектным типом.",$"Type {targetType} is not an object type.");
        
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