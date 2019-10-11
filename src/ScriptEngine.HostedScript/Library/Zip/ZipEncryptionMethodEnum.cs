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
    [SystemEnum("МетодШифрованияZIP", "ZIPEncryptionMethod")]
    public class ZipEncryptionMethodEnum : EnumerationContext
    {
        private const string EV_AES128 = "AES128";
        private const string EV_AES192 = "AES192";
        private const string EV_AES256 = "AES256";
        private const string EV_ZIP20 = "Zip20";

        private ZipEncryptionMethodEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
        }

        [EnumValue(EV_AES128)]
        public EnumerationValue Aes128
        {
            get
            {
                return this[EV_AES128];
            }
        }

        [EnumValue(EV_AES192)]
        public EnumerationValue Aes192
        {
            get
            {
                return this[EV_AES192];
            }
        }

        [EnumValue(EV_AES256)]
        public EnumerationValue Aes256
        {
            get
            {
                return this[EV_AES256];
            }
        }

        [EnumValue(EV_ZIP20)]
        public EnumerationValue Zip20
        {
            get
            {
                return this[EV_ZIP20];
            }
        }

        public static ZipEncryptionMethodEnum CreateInstance()
        {
            return EnumContextHelper.CreateEnumInstance<ZipEncryptionMethodEnum>((t, v) => new ZipEncryptionMethodEnum(t, v));
        }
    }
}
