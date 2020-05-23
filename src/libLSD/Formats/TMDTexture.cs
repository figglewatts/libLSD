using System.IO;
using libLSD.Interfaces;
using libLSD.Util;

namespace libLSD.Formats
{
    /// <summary>
    /// Contains information on a texture/sprite on a TMD primitive packet.
    /// </summary>
    public struct TMDTexture : IWriteable
    {
        /// <summary>
        /// Algorithms describing how to apply transparency.
        /// </summary>
        public enum TransparencyRates
        {
            RATE_50BACK_PLUS_50POLY = 0,    // 50% back + 50% poly
            RATE_100BACK_PLUS_100POLY = 1,  // 100% back + 100% poly
            RATE_100BACK_MINUS_100POLY = 2, // 100% back - 100% poly
            RATE_100BACK_MINUS_25POLY = 3   // 100% back - 25% poly
        }

        /// <summary>
        /// Color modes for textures.
        /// </summary>
        public enum ColorModes
        {
            FOURBIT = 0,
            EIGHTBIT = 1,
            FIFTEENBIT = 2
        }

        /// <summary>
        /// The texture page to get the texture from.
        /// </summary>
        public int TexturePageNumber
        {
            get => _tsb & TPAGE_MASK;
            private set => _tsb = BitTwiddling.Merge<ushort>(_tsb, value, TPAGE_MASK);
        }

        /// <summary>
        /// The semi-transparency algorithm.
        /// </summary>
        public TransparencyRates AlphaBlendRate
        {
            get => (TransparencyRates)((_tsb & ABR_MASK) >> 5);
            private set => _tsb = BitTwiddling.Merge<ushort>(_tsb, value, ABR_MASK, 5);
        }

        /// <summary>
        /// The color mode of the texture data.
        /// </summary>
        public ColorModes ColorMode
        {
            get => (ColorModes)((_tsb & TPF_MASK) >> 7);
            private set => _tsb = BitTwiddling.Merge<ushort>(_tsb, value, TPF_MASK, 7);
        }

        private const ushort TPAGE_MASK = 0b11111;
        private const ushort ABR_MASK = 0b1100000;
        private const ushort TPF_MASK = 0b110000000;

        private ushort _tsb;

        /// <summary>
        /// Create a new TMDTexture from a short.
        /// </summary>
        /// <param name="tsb">The short.</param>
        private TMDTexture(ushort tsb) { _tsb = tsb; }

        /// <summary>
        /// Convert this TMDTexture into its short representation.
        /// </summary>
        /// <param name="tex">The TMDTexture.</param>
        /// <returns>The short.</returns>
        public static explicit operator ushort(TMDTexture tex) { return tex._tsb; }

        /// <summary>
        /// Convert a short representation into a TMDTexture.
        /// </summary>
        /// <param name="tsb">The short.</param>
        /// <returns>A TMDTexture.</returns>
        public static implicit operator TMDTexture(ushort tsb) { return new TMDTexture(tsb); }

        /// <summary>
        /// Write this TMDTexture to a binary stream.
        /// </summary>
        /// <param name="bw">The binary stream.</param>
        public void Write(BinaryWriter bw) { bw.Write(_tsb); }
    }
}
