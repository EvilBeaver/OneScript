/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace ScriptEngine.HostedScript.Library
{
	[EnumerationType("ЧастиДаты", "DateFractions")]
	public enum DateFractionsEnum
	{
		[EnumItem("Дата", "Date")]
		Date,

		[EnumItem("Время", "Time")]
		Time,

		[EnumItem("ДатаВремя", "DateTime")]
		DateTime
	}
}
