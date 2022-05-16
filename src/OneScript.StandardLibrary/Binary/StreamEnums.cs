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
        [EnumValue("Дописать")]
        Append,
        [EnumValue("Обрезать")]
        Truncate,
        [EnumValue("Открыть")]
        Open,
        [EnumValue("ОткрытьИлиСоздать")]
        OpenOrCreate,
        [EnumValue("Создать")]
        Create,
        [EnumValue("СоздатьНовый")]
        CreateNew
    }

    [EnumerationType("ДоступКФайлу", "FileAccess")]
    public enum FileAccessEnum
    {
        [EnumValue("Запись")]
        Write,
        [EnumValue("Чтение")]
        Read,
        [EnumValue("ЧтениеИЗапись")]
        ReadAndWrite
    }

    [EnumerationType("ПозицияВПотоке", "StreamPosition")]
    public enum StreamPositionEnum
    {
        [EnumValue("Начало")]
        Begin,
        [EnumValue("Конец")]
        End,
        [EnumValue("Текущая")]
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
