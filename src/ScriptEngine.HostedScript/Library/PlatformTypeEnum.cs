/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace ScriptEngine.HostedScript.Library
{
    [EnumerationType("ТипПлатформы", "PlatformType")]
    public enum PlatformTypeEnum
    {
        [EnumItem("Windows_x86")]
        Windows_x86,

        [EnumItem("Windows_x86_64")]
        Windows_x86_64,

        [EnumItem("Linux_x86")]
        Linux_x86,
        
        [EnumItem("Linux_x86_64")]
        Linux_x86_64,
        
        [EnumItem("MacOS_x86")]
        MacOS_x86,

        [EnumItem("MacOS_x86_64")]
        MacOS_x86_64, 

        [EnumItem("Unknown")]
        Unknown,
    }
}
