/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts.Enums;

namespace OneScript.StandardLibrary.TypeDescriptions
{
	[EnumerationType("ДопустимаяДлина", "AllowedLength")]
	public enum AllowedLengthEnum
	{
		[EnumValue("Переменная", "Variable")]
		Variable,

		[EnumValue("Фиксированная", "Fixed")]
		Fixed
	}
}
