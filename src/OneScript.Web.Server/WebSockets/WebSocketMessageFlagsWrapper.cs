/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using OneScript.Contexts.Enums;

namespace OneScript.Web.Server.WebSockets
{
    [EnumerationType("ФлагиСообщенияВебСокета", "WebSocketMessageFlags")]
    public enum WebSocketMessageFlagsWrapper
    {
        [EnumValue("Пусто", "None")]
        None = 0,
        [EnumValue("КонецСообщения", "EndOfMessage")]
        EndOfMessage = 1,
        [EnumValue("ОтключитьСжатие", "DisableCompression")]
        DisableCompression = 2
    }
}
