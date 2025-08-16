/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts.Enums;

namespace OneScript.StandardLibrary.Security.Tokens
{
    /// <summary>
    /// Алгоритмы подписи токена доступа.
    /// </summary>
    [EnumerationType("АлгоритмПодписиТокенаДоступа", "AccessTokenSignAlgorithm")]
    public enum AccessTokenSignAlgorithmEnum
    {
        [EnumValue("ES256")]
        ES256,
        
        [EnumValue("ES384")]
        ES384,
        
        [EnumValue("ES512")]
        ES512,
        
        [EnumValue("HS256")]
        HS256,
        
        [EnumValue("HS384")]
        HS384,
        
        [EnumValue("HS512")]
        HS512,
        
        [EnumValue("PS256")]
        PS256,
        
        [EnumValue("PS384")]
        PS384,
        
        [EnumValue("PS512")]
        PS512,
        
        [EnumValue("RS256")]
        RS256,
        
        [EnumValue("RS384")]
        RS384,
        
        [EnumValue("RS512")]
        RS512,
        
        [EnumValue("Нет", "None")]
        None
    }
}
