// /*----------------------------------------------------------
// This Source Code Form is subject to the terms of the 
// Mozilla Public License, v.2.0. If a copy of the MPL 
// was not distributed with this file, You can obtain one 
// at http://mozilla.org/MPL/2.0/.
// ----------------------------------------------------------*/
// /*----------------------------------------------------------
// This Source Code Form is subject to the terms of the 
// Mozilla Public License, v.2.0. If a copy of the MPL 
// was not distributed with this file, You can obtain one 
// at http://mozilla.org/MPL/2.0/.
// ----------------------------------------------------------*/
namespace ScriptEngine.HostedScript.Library.Zip
{
    [EnumerationType("КодировкаИменФайловВZipФайле","FileNamesEncodingInZipFile")]
    public enum FileNamesEncodingInZipFile
    {
        [EnumItem("Авто")]
        Auto,
        
        [EnumItem("UTF8")]
        Utf8,
        
        [EnumItem("КодировкаОСДополнительноUTF8","OSEncodingWithUTF8")]
        OsEncodingWithUtf8
    }
}