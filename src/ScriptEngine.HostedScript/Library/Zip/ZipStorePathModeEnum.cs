/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript.Library.Zip
{
    [SystemEnum("РежимСохраненияПутейZIP", "ZIPStorePathsMode")]
    public class ZipStorePathModeEnum : EnumerationContext
    {
        const string DONT_SAVE = "НеСохранятьПути";
        const string SAVE_RELATIVE = "СохранятьОтносительныеПути";
        const string SAVE_FULL = "СохранятьПолныеПути";

        public ZipStorePathModeEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {

        }

        [EnumValue(DONT_SAVE, "DontStorePath")]
        public EnumerationValue DontStorePath
        {
            get
            {
                return this[DONT_SAVE];
            }
        }

        [EnumValue(SAVE_RELATIVE, "StoreRelativePath")]
        public EnumerationValue StoreRelativePath
        {
            get
            {
                return this[SAVE_RELATIVE];
            }
        }

        [EnumValue(SAVE_FULL, "StoreFullPath")]
        public EnumerationValue StoreFullPath
        {
            get
            {
                return this[SAVE_FULL];
            }
        }

        public static ZipStorePathModeEnum CreateInstance()
        {
             return EnumContextHelper.CreateEnumInstance<ZipStorePathModeEnum>((t, v) => new ZipStorePathModeEnum(t, v));
        }
    }
}
