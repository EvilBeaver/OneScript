/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts.Enums;

namespace OneScript.StandardLibrary.Binary
{
    [EnumerationType("РежимОткрытияФайла", "FileOpenMode")]
    public enum FileOpenModeEnum
    {
        [EnumValue("Дописать", "Append")]
        Append,
        [EnumValue("Обрезать", "Truncate")]
        Truncate,
        [EnumValue("Открыть", "Open")]
        Open,
        [EnumValue("ОткрытьИлиСоздать", "OpenOrCreate")]
        OpenOrCreate,
        [EnumValue("Создать", "Create")]
        Create,
        [EnumValue("СоздатьНовый", "CreateNew")]
        CreateNew
    }

    [EnumerationType("ДоступКФайлу", "FileAccess")]
    public enum FileAccessEnum
    {
        [EnumValue("Запись", "Write")]
        Write,
        [EnumValue("Чтение", "Read")]
        Read,
        [EnumValue("ЧтениеИЗапись", "ReadAndWrite")]
        ReadAndWrite
    }

    [EnumerationType("ПозицияВПотоке", "StreamPosition")]
    public enum StreamPositionEnum
    {
        [EnumValue("Начало", "Begin")]
        Begin,
        [EnumValue("Конец", "End")]
        End,
        [EnumValue("Текущая", "Current")]
        Current
    }

    [EnumerationType("ПорядокБайтов", "ByteOrder")]
    public enum ByteOrderEnum
    {
        [EnumValue("BigEndian")]
        BigEndian,
        [EnumValue("LittleEndian")]
        LittleEndian
    }
}
