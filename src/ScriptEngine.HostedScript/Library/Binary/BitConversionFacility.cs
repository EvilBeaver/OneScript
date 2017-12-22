using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
