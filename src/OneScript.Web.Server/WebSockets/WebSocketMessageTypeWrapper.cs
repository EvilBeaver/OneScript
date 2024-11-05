/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using OneScript.Contexts.Enums;

namespace OneScript.Web.Server.WebSockets
{
    [EnumerationType("ТипСообщенияВебСокета", "WebSocketMessageType")]
    public enum WebSocketMessageTypeWrapper
    {
        [EnumValue("Текст", "Text")]
        Text = 0,
        [EnumValue("ДвоичныеДанные", "Binary")]
        Binary = 1,
        [EnumValue("Закрытие", "Close")]
        Close = 2
    }
}
