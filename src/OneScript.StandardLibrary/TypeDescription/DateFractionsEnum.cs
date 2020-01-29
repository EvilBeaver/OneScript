/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine;

namespace OneScript.StandardLibrary.TypeDescription
{
	[EnumerationType("ЧастиДаты", "DateFractions")]
	public enum DateFractionsEnum
	{
		[EnumItem("Дата", "Date")]
		Date,

		[EnumItem("ДатаВремя", "DateTime")]
		DateTime,

		[EnumItem("Время", "Time")]
		Time
	}
}
