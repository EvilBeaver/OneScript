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
        [EnumItem("Дописать")]
        Append,
        [EnumItem("Обрезать")]
        Truncate,
        [EnumItem("Открыть")]
        Open,
        [EnumItem("ОткрытьИлиСоздать")]
        OpenOrCreate,
        [EnumItem("Создать")]
        Create,
        [EnumItem("СоздатьНовый")]
        CreateNew
    }

    [EnumerationType("ДоступКФайлу", "FileAccess")]
    public enum FileAccessEnum
    {
        [EnumItem("Запись")]
        Write,
        [EnumItem("Чтение")]
        Read,
        [EnumItem("ЧтениеИЗапись")]
        ReadAndWrite
    }

    [EnumerationType("ПозицияВПотоке", "StreamPosition")]
    public enum StreamPositionEnum
    {
        [EnumItem("Начало")]
        Begin,
        [EnumItem("Конец")]
        End,
        [EnumItem("Текущая")]
        Current
    }

    [EnumerationType("ПорядокБайтов", "ByteOrder")]
    public enum ByteOrderEnum
    {
        [EnumItem("BigEndian")]
        BigEndian,
        [EnumItem("LittleEndian")]
        LittleEndian
    }
}
