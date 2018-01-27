using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libLSD.Types
{
    public struct Color16Bit : IColor
    {
        public uint Red => _colorData & 0b11111;
        public uint Green => (_colorData >> 5) & 0b11111;
        public uint Blue => (_colorData >> 10) & 0b11111;
        public bool Transparency => ((_colorData >> 15) & 0x1) == 1;

        private readonly uint _colorData;

        public Color16Bit(BinaryReader br)
        {
            _colorData = br.ReadUInt16();
        }

        public Color16Bit(ushort data)
        {
            _colorData = data;
        }
    }
}
