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
        public float Red => _red / 31f;
        public float Green => _green / 31f;
        public float Blue => _blue / 31f;
        public float Alpha { get; }

        private byte _red => (byte)(_colorData & 0b11111);
        private byte _green => (byte)((_colorData >> 5) & 0b11111);
        private byte _blue => (byte)((_colorData >> 10) & 0b11111);

        private bool _stp => ((_colorData >> 15) & 0x1) == 1;

        private bool IsBlack => _red == 0 && _green == 0 && _blue == 0;

        private readonly uint _colorData;

        public Color16Bit(BinaryReader br)
        {
            _colorData = br.ReadUInt16();
            Alpha = 1;

            if (_stp && IsBlack)
            {
                Alpha = 1;
            }
            else if (_stp && !IsBlack)
            {
                Alpha = 0.5f;
            }
            else if (!_stp && IsBlack)
            {
                Alpha = 0;
            }
        }

        public Color16Bit(ushort data)
        {
            _colorData = data;

            Alpha = 1;

            if (_stp && IsBlack)
            {
                Alpha = 1;
            }
            else if (_stp && !IsBlack)
            {
                Alpha = 0.5f;
            }
            else if (!_stp && IsBlack)
            {
                Alpha = 0;
            }
        }
    }
}
