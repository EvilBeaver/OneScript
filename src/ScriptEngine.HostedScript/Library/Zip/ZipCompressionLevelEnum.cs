﻿/*----------------------------------------------------------
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
    [SystemEnum("УровеньСжатияZIP", "ZIPCompressionLevel")]
    public class ZipCompressionLevelEnum : EnumerationContext
    {
        private const string EV_MINIMAL_NAME = "Минимальный";
        private const string EV_OPTIMAL_NAME = "Оптимальный";
        private const string EV_MAXIMAL_NAME = "Максимальный";

        private ZipCompressionLevelEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
        }
        
        [EnumValue(EV_MINIMAL_NAME, "Minimum")]
        public EnumerationValue Minimal
        {
            get
            {
                return this[EV_MINIMAL_NAME];
            }
        }

        [EnumValue(EV_OPTIMAL_NAME, "Optimal")]
        public EnumerationValue Optimal
        {
            get
            {
                return this[EV_OPTIMAL_NAME];
            }
        }

        [EnumValue(EV_MAXIMAL_NAME, "Maximum")]
        public EnumerationValue Maximal
        {
            get
            {
                return this[EV_MAXIMAL_NAME];
            }
        }

        public static ZipCompressionLevelEnum CreateInstance()
        {
            return EnumContextHelper.CreateEnumInstance<ZipCompressionLevelEnum>((t, v) => new ZipCompressionLevelEnum(t, v));
        }
    }
}
