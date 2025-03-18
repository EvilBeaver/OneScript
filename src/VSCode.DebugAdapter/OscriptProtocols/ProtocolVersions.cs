/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace VSCode.DebugAdapter.OscriptProtocols
{
    public static class ProtocolVersions
    {
        public static bool IsValid(int valueToCheck)
        {
            return valueToCheck >= 0 && valueToCheck <= LatestKnownVersion;
        }

        public static int Adjust(int valueToAdjust)
        {
            if (valueToAdjust < 0 || valueToAdjust > LatestKnownVersion)
                return SafestVersion;

            return valueToAdjust;
        }
        
        /// <summary>
        /// Неизвестная версия, пытаемся определить сами
        /// </summary>
        public const int UnknownVersion = 0;
        
        /// <summary>
        /// До появления условных брейкпоинтов
        /// </summary>
        public const int Version1 = 1;
 
        /// <summary>
        /// После появления условных брейкпоинтов, но до появления метода GetVersion
        /// </summary>
        public const int Version2 = 2;

        /// <summary>
        /// После появления метода GetVersion, который запрашивает Debuggee
        /// </summary>
        public const int Version3 = 3;

        /// <summary>
        /// Значение, безопасное для всех версий движка
        /// </summary>
        public const int SafestVersion = Version1;
        
        /// <summary>
        /// Контрольное значение
        /// </summary>
        public const int LatestKnownVersion = Version3;
    }
}