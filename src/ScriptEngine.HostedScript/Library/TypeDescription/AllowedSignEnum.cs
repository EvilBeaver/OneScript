/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace ScriptEngine.HostedScript.Library
{
	[EnumerationType("ДопустимыйЗнак", "AllowedSign")]
	public enum AllowedSignEnum
	{
		[EnumItem("Любой", "Any")]
		Any,

		[EnumItem("Неотрицательный", "Nonnegative")]
		Nonnegative
	}
}
