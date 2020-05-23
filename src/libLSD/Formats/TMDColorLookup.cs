using System.IO;
using libLSD.Interfaces;
using libLSD.Util;

namespace libLSD.Formats
{
    /// <summary>
    /// Indicates the position in VRAM where a color lookup table is stored.
    /// </summary>
    public struct TMDColorLookup : IWriteable
    {
        /// <summary>
        /// The X position in VRAM.
        /// </summary>
        public int XPosition
        {
            get => _cba & CLX_MASK;
            private set => _cba = BitTwiddling.Merge<ushort>(_cba, value, CLX_MASK);
        }

        /// <summary>
        /// The Y position in VRAM.
        /// </summary>
        public int YPosition
        {
            get => (_cba & CLY_MASK) >> 6;
            private set => _cba = BitTwiddling.Merge<ushort>(_cba, value, CLY_MASK, 6);
        }

        private const ushort CLX_MASK = 0b111111;
        private const ushort CLY_MASK = 0b111111111000000;

        private ushort _cba;

        private TMDColorLookup(ushort cba) { _cba = cba; }

        /// <summary>
        /// Convert this TMDColorLookup into it's 2-byte representation.
        /// </summary>
        /// <param name="cl">The TMDColorLookup.</param>
        /// <returns>The 2-byte short representation.</returns>
        public static explicit operator ushort(TMDColorLookup cl) { return cl._cba; }

        /// <summary>
        /// Convert to a TMDColorLookup from a 2-byte short.
        /// </summary>
        /// <param name="cba">The 2-byte short.</param>
        /// <returns>The TMDColorLookup.</returns>
        public static implicit operator TMDColorLookup(ushort cba) { return new TMDColorLookup(cba); }

        /// <summary>
        /// Write this TMDColorLookup to a binary stream.
        /// </summary>
        /// <param name="bw">The binary stream.</param>
        public void Write(BinaryWriter bw) { bw.Write(_cba); }
    }
}
