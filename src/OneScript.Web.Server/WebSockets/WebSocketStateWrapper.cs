/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using OneScript.Contexts.Enums;

namespace OneScript.Web.Server.WebSockets
{
    [EnumerationType("СостояниеВебСокета", "WebSocketState")]
    public enum WebSocketStateWrapper
    {
        [EnumValue("Пусто", "None")]
        None = 0,
        [EnumValue("Подключение", "Connecting")]
        Connecting = 1,
        [EnumValue("Открыт", "Open")]
        Open = 2,
        [EnumValue("ЗапросЗакрытияОтправлен", "CloseSent")]
        CloseSent = 3,
        [EnumValue("ЗапросЗакрытияПринят", "CloseReceived")]
        CloseReceived = 4,
        [EnumValue("Закрыт", "Closed")]
        Closed = 5,
        [EnumValue("Прерван", "Aborted")]
        Aborted = 6,
    }
}
