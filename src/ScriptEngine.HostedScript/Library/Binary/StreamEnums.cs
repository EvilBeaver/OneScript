using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        [EnumItem("Конец")]
        End,
        [EnumItem("Начало")]
        Begin,
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
