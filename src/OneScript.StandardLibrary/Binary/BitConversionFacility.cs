/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace ScriptEngine.HostedScript.Library.Binary
{
    public static class BitConversionFacility
    {
        static BitConversionFacility()
        {
            LittleEndian = new EndianBitConverter()
            {
                IsLittleEndian = true
            };

            BigEndian = new EndianBitConverter();
        }

        public static EndianBitConverter LittleEndian { get; set; }
        public static EndianBitConverter BigEndian { get; set; }
    }
}
