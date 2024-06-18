/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using OneScript.Contexts.Enums;

namespace OneScript.Web.Server.WebSockets
{
    [EnumerationType("СтатусЗакрытияВебСокета", "WebSocketCloseStatus")]
    public enum WebSocketCloseStatusWrapper
    {
        [EnumValue("НормальноеЗакрытие", "NormalClosure")]
        NormalClosure = 1000,
        [EnumValue("НедоступностьКонечнойТочки", "EndpointUnavailable")]
        EndpointUnavailable = 1001,
        [EnumValue("ОшибкаПротокола", "ProtocolError")]
        ProtocolError = 1002,
        [EnumValue("НеверныйТипСообщения", "InvalidMessageType")]
        InvalidMessageType = 1003,
        [EnumValue("Пустой", "Empty")]
        Empty = 1005,
        [EnumValue("НеверныеДанныеСообщения", "InvalidPayloadData")]
        InvalidPayloadData = 1007,
        [EnumValue("НарушениеПолитики", "PolicyViolation")]
        PolicyViolation = 1008,
        [EnumValue("СлишкомБольшоеСообщение", "MessageTooBig")]
        MessageTooBig = 1009,
        [EnumValue("СогласованиеРасширения", "MandatoryExtension")]
        MandatoryExtension = 1010,
        [EnumValue("ВнутренняяОшибкаСервера", "InternalServerError")]
        InternalServerError = 1011
    }
}
