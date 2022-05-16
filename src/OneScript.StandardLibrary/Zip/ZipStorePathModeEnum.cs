/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts.Enums;

namespace OneScript.StandardLibrary.Zip
{
    [EnumerationType("РежимСохраненияПутейZIP", "ZIPStorePathsMode")]
    public enum ZipStorePathMode
    {
        [EnumValue("НеСохранятьПути", "DontStorePath")]
        DontStorePath,
        [EnumValue("СохранятьОтносительныеПути", "StoreRelativePath")]
        StoreRelativePath,
        [EnumValue("СохранятьПолныеПути", "StoreFullPath")]
        StoreFullPath
    }
}
