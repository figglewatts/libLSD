using System.Diagnostics.Contracts;
using System.IO;
using libLSD.Interfaces;
using libLSD.Util;

namespace libLSD.Formats
{
    public struct TMDColorLookup : IWriteable
    {
        public int XPosition
        {
            get => _cba & CLX_MASK;
            private set => _cba = BitTwiddling.Merge<ushort>(_cba, value, CLX_MASK);
        }

        public int YPosition
        {
            get => (_cba & CLY_MASK) >> 6;
            private set => _cba = BitTwiddling.Merge<ushort>(_cba, value, CLY_MASK, 6);
        }

        private const ushort CLX_MASK = 0b111111;
        private const ushort CLY_MASK = 0b111111111000000;

        private ushort _cba;

        private TMDColorLookup(ushort cba) { _cba = cba; }

        public static explicit operator ushort(TMDColorLookup cl) { return cl._cba; }

        public static implicit operator TMDColorLookup(ushort cba) { return new TMDColorLookup(cba); }

        public void Write(BinaryWriter bw)
        {

        }
    }
}
