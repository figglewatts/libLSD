using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libLSD.Types
{
    public struct Color24Bit : IColor
    {
        public uint Red { get; }
        public uint Green { get; }
        public uint Blue { get; }

        public Color24Bit(ushort r, ushort g, ushort b)
        {
            Red = r;
            Green = g;
            Blue = b;
        }
    }
}
