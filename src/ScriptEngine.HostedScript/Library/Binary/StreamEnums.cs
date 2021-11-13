/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace ScriptEngine.HostedScript.Library.Binary
{
    [EnumerationType("РежимОткрытияФайла", "FileOpenMode")]
    public enum FileOpenModeEnum
    {
        [EnumItem("Создать", "Create")]
        Create,
        [EnumItem("СоздатьНовый", "CreateNew")]
        CreateNew,
        [EnumItem("Открыть", "Open")]
        Open,
        [EnumItem("ОткрытьИлиСоздать", "OpenOrCreate")]
        OpenOrCreate,
        [EnumItem("Обрезать", "Truncate")]
        Truncate,
        [EnumItem("Дописать", "Append")]
        Append
    }

    [EnumerationType("ДоступКФайлу", "FileAccess")]
    public enum FileAccessEnum
    {
        [EnumItem("Чтение", "Read")]
        Read,
        [EnumItem("Запись", "Write")]
        Write,
        [EnumItem("ЧтениеИЗапись", "ReadAndWrite")]
        ReadAndWrite
    }

    [EnumerationType("ПозицияВПотоке", "StreamPosition")]
    public enum StreamPositionEnum
    {
        [EnumItem("Начало", "Begin")]
        Begin,
        [EnumItem("Текущая", "Current")]
        Current,
        [EnumItem("Конец", "End")]
        End
    }

    [EnumerationType("ПорядокБайтов", "ByteOrder")]
    public enum ByteOrderEnum
    {
        [EnumItem("LittleEndian")]
        LittleEndian,
        [EnumItem("BigEndian")]
        BigEndian
    }
}
