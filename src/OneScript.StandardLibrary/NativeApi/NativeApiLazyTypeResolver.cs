/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Types;

namespace OneScript.StandardLibrary.NativeApi
{
    /// <summary>
    /// Разрешает имена типов внешних компонент вида AddIn.&lt;метка&gt;.&lt;имя&gt;
    /// для подключённых библиотек без предварительного перечисления GetClassNames.
    /// </summary>
    /// <remarks>
    /// Осознанные ограничения:
    /// 1. Опечатка в имени класса не является ошибкой на уровне Тип() — валидный дескриптор
    ///    возвращается для подключённой метки, а исключение возникает только при Новый().
    /// 2. Ошибочные имена навсегда остаются в реестре типов: ITypeManager не умеет
    ///    разрегистрировать тип.
    /// 3. IsKnownType и RegisteredTypes не видят неразрешённые имена до первого обращения.
    /// 4. Дескриптор регистрируется в том написании, в котором имя запросили первым.
    /// </remarks>
    class NativeApiLazyTypeResolver : ILazyTypeResolver
    {
        public bool TryResolve(string typeName, ITypeManager typeManager, out TypeDescriptor type)
        {
            var parts = typeName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
            {
                type = default;
                return false;
            }

            if (!string.Equals(parts[0], "AddIn", StringComparison.OrdinalIgnoreCase))
            {
                type = default;
                return false;
            }

            if (!NativeApiFactory.TryGetLibrary(parts[1], out _))
            {
                type = default;
                return false;
            }

            type = typeManager.RegisterType(typeName, default, typeof(NativeApiFactory));
            return true;
        }
    }
}
