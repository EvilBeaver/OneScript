/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts;
using OneScript.Contexts.Enums;

namespace OneScript.Core.Tests
{
    [DeprecatedName("СтароеИмя")]
    [EnumerationType("НовоеПеречисление")]
    public enum DeprecatedEnum
    {
        [EnumValue("Значение1")]
        Value1,
        [DeprecatedName("СтароеЗначение2")]
        [EnumValue("Значение2")]
        Value2
    }
}