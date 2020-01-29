/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Zip
{
    [SystemEnum("РежимВосстановленияПутейФайловZIP", "ZIPRestoreFilePathsMode")]
    public class ZipRestoreFilePathsModeEnum : EnumerationContext
    {
        private const string RESTORE_PATHS_NAME = "Восстанавливать";
        private const string DONT_RESTORE_PATHS_NAME = "НеВосстанавливать";

        private ZipRestoreFilePathsModeEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
        }

        [EnumValue(RESTORE_PATHS_NAME, "Restore")]
        public EnumerationValue Restore
        {
            get
            {
                return this[RESTORE_PATHS_NAME];
            }
        }

        [EnumValue(DONT_RESTORE_PATHS_NAME, "DontRestore")]
        public EnumerationValue DoNotRestore
        {
            get
            {
                return this[DONT_RESTORE_PATHS_NAME];
            }
        }

        public static ZipRestoreFilePathsModeEnum CreateInstance()
        {
            return EnumContextHelper.CreateEnumInstance<ZipRestoreFilePathsModeEnum>((t, v) => new ZipRestoreFilePathsModeEnum(t, v));
        }
    }
}
