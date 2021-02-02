/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;

namespace OneScript.StandardLibrary.DriveInfo
{
    /// <summary>
    /// Типы дисков:
    /// - Диск является устройством оптических дисков, такие как компакт-ДИСК или DVD-диск.
    /// - Диск является жестким диском.
    /// - Диск является сетевым диском.
    /// - Диск не имеет корневой каталог.
    /// - Диск является диском ОЗУ.
    /// - Диск является съемное запоминающее устройство, например, дисковод гибких дисков или USB-устройство флэш-памяти.
    /// - Тип диска неизвестен.
    /// </summary>
    [SystemEnum("ТипДиска", "DriveType")]
    public class DriveTypeEnum : EnumerationContext
    {

        private DriveTypeEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {

        }

        public static DriveTypeEnum CreateInstance(ITypeManager typeManager)
        {
            
            var instance = EnumContextHelper.CreateClrEnumInstance<DriveTypeEnum, System.IO.DriveType>(
                typeManager,
                (t, v) => new DriveTypeEnum(t, v)); 
            
            instance.WrapClrValue("Неизвестный", "Unknown", System.IO.DriveType.Unknown);
            instance.WrapClrValue("НеИмеетКорневойКаталог", "NoRootDirectory", System.IO.DriveType.NoRootDirectory);
            instance.WrapClrValue("СъемноеЗапоминающееУстройство", "Removable", System.IO.DriveType.Removable);
            instance.WrapClrValue("ЖесткийДиск", "Fixed", System.IO.DriveType.Fixed);
            instance.WrapClrValue("СетевойДиск", "Network", System.IO.DriveType.Network);
            instance.WrapClrValue("ОптическийДиск", "CDRom", System.IO.DriveType.CDRom);
            instance.WrapClrValue("ДискОЗУ", "Ram", System.IO.DriveType.Ram);

            return instance;
        }

    }

}
