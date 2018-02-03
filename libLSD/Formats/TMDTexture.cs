using System.IO;
using libLSD.Interfaces;
using libLSD.Util;

namespace libLSD.Formats
{
    public struct TMDTexture : IWriteable
    {
        public enum TransparencyRates
        {
            RATE_50BACK_PLUS_50POLY = 0,    // 50% back + 50% poly
            RATE_100BACK_PLUS_100POLY = 1,  // 100% back + 100% poly
            RATE_100BACK_MINUS_100POLY = 2, // 100% back - 100% poly
            RATE_100BACK_MINUS_25POLY = 3   // 100% back - 25% poly
        }

        public enum ColorModes
        {
            FOURBIT = 0,
            EIGHTBIT = 1,
            FIFTEENBIT = 2
        }

        public int TexturePageNumber
        {
            get => _tsb & TPAGE_MASK;
            private set => _tsb = BitTwiddling.Merge<ushort>(_tsb, value, TPAGE_MASK);
        }

        public TransparencyRates AlphaBlendRate
        {
            get => (TransparencyRates)((_tsb & ABR_MASK) >> 5);
            private set => _tsb = BitTwiddling.Merge<ushort>(_tsb, value, ABR_MASK, 5);
        }

        public ColorModes ColorMode
        {
            get => (ColorModes)((_tsb & TPF_MASK) >> 7);
            private set => _tsb = BitTwiddling.Merge<ushort>(_tsb, value, TPF_MASK, 7);
        }

        private const ushort TPAGE_MASK = 0b11111;
        private const ushort ABR_MASK = 0b1100000;
        private const ushort TPF_MASK = 0b110000000;

        private ushort _tsb;

        private TMDTexture(ushort tsb) { _tsb = tsb; }

        public static explicit operator ushort(TMDTexture tex) { return tex._tsb; }

        public static implicit operator TMDTexture(ushort tsb) { return new TMDTexture(tsb); }

        public void Write(BinaryWriter bw)
        {

        }
    }
}
