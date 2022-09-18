/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts.Enums;

namespace OneScript.StandardLibrary.Hash
{
    [EnumerationType("ХешФункция", "HashFunction",
        TypeUUID = "FCD487D9-9C9A-423D-B4E8-90FB3248AACF",
        ValueTypeUUID = "39B708B2-98ED-4B14-B112-60985A547526")]
    public enum HashFunctionEnum
    {
        [EnumValue("MD5")]
        MD5,
        
        [EnumValue("SHA1")]
        SHA1,
        
        [EnumValue("SHA256")]
        SHA256,
        
        [EnumValue("SHA384")]
        SHA384,
        
        [EnumValue("SHA512")]
        SHA512,
        
        [EnumValue("CRC32")]
        CRC32

    }
}
