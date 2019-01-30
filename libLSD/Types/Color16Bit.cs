using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libLSD.Interfaces;
using libLSD.Util;

namespace libLSD.Types
{
    public struct Color16Bit : IColor, IWriteable
    {
        public float Red => _red / 31f;
        public float Green => _green / 31f;
        public float Blue => _blue / 31f;
        public float Alpha => _alpha / 31f;
        public bool TransparencyControl => _stp;

        private byte _red => (byte)(_colorData & 0b11111);
        private byte _green => (byte)((_colorData >> 5) & 0b11111);
        private byte _blue => (byte)((_colorData >> 10) & 0b11111);

        private byte _alpha;

        private bool _stp => ((_colorData >> 15) & 0x1) == 1;

        public bool IsBlack => _red == 0 && _green == 0 && _blue == 0;

        private readonly uint _colorData;

        public Color16Bit(BinaryReader br)
        {
            _colorData = br.ReadUInt16();
            _alpha = 0b11111;
            if (TransparencyControl)
            {
                if (!IsBlack)
                {
                    _alpha = Math.Max(_red, Math.Max(_blue, _green));
                }
            }
            else
            {
                if (IsBlack)
                {
                    _alpha = 0;
                }
            }
            /*if (IsBlack && !TransparencyControl)
            {
                Alpha = 0;
            }*/
        }

        public Color16Bit(ushort data)
        {
            _colorData = data;
            _alpha = 0b11111;
            if (TransparencyControl)
            {
                if (!IsBlack)
                {
                    _alpha = Math.Max(_red, Math.Max(_blue, _green));
                }
            }
            else
            {
                if (IsBlack)
                {
                    _alpha = 0;
                }
            }
            /*if (IsBlack && !TransparencyControl)
            {
                Alpha = 0;
            }*/
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write((ushort)_colorData);
        }
    }
}
