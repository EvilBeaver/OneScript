/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts.Enums;

namespace OneScript.StandardLibrary.Zip
{
    [EnumerationType("МетодШифрованияZIP", "ZIPEncryptionMethod")]
    public enum ZipEncryptionMethod
    {
        [EnumValue("AES128")]
        Aes128,
        [EnumValue("AES192")]
        Aes192,
        [EnumValue("AES256")]
        Aes256,
        [EnumValue("Zip20")]
        Zip20
    }
}
