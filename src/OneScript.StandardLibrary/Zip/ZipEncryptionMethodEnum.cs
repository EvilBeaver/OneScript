/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine;

namespace OneScript.StandardLibrary.Zip
{
    [EnumerationType("МетодШифрованияZIP", "ZIPEncryptionMethod")]
    public enum ZipEncryptionMethod
    {
        [EnumItem("AES128")]
        Aes128,
        [EnumItem("AES192")]
        Aes192,
        [EnumItem("AES256")]
        Aes256,
        [EnumItem("Zip20")]
        Zip20
    }
}
