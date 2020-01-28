/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace ScriptEngine.HostedScript.Library
{
    [EnumerationType("СтатусСообщения", "MessageStatus")]
	public enum MessageStatusEnum
    {
        [EnumItem("БезСтатуса", "WithoutStatus")]
		WithoutStatus,

		[EnumItem("Важное", "Important")]
		Important,

		[EnumItem("Внимание", "Attention")]
		Attention,

		[EnumItem("Информация", "Information")]
		Information,

		[EnumItem("Обычное", "Ordinary")]
		Ordinary,

		[EnumItem("ОченьВажное", "VeryImportant")]
		VeryImportant

    }
}
