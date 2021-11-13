/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.Zip
{
    [SystemEnum("РежимСохраненияПутейZIP", "ZIPStorePathsMode")]
    public class ZipStorePathModeEnum : EnumerationContext
    {
        const string SAVE_FULL = "СохранятьПолныеПути";
        const string SAVE_RELATIVE = "СохранятьОтносительныеПути";
        const string DONT_SAVE = "НеСохранятьПути";

        public ZipStorePathModeEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {

        }

        [EnumValue(SAVE_FULL, "StoreFullPath")]
        public EnumerationValue StoreFullPath => this[SAVE_FULL];

        [EnumValue(SAVE_RELATIVE, "StoreRelativePath")]
        public EnumerationValue StoreRelativePath => this[SAVE_RELATIVE];

        [EnumValue(DONT_SAVE, "DontStorePath")]
        public EnumerationValue DontStorePath => this[DONT_SAVE];

        public static ZipStorePathModeEnum CreateInstance()
        {
             return EnumContextHelper.CreateEnumInstance<ZipStorePathModeEnum>((t, v) => new ZipStorePathModeEnum(t, v));
        }
    }
}
