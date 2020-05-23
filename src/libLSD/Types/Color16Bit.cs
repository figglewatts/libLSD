using System;
using System.IO;
using libLSD.Interfaces;

namespace libLSD.Types
{
    /// <summary>
    /// A 16-bit color, as stored on the PSX.
    /// </summary>
    public struct Color16Bit : IColor, IWriteable
    {
        /// <summary>
        /// The Red component of the color, in range [0, 1].
        /// </summary>
        public float Red => _red / 31f;

        /// <summary>
        /// The Green component of the color, in range [0, 1].
        /// </summary>
        public float Green => _green / 31f;

        /// <summary>
        /// The Blue component of the color, in range [0, 1].
        /// </summary>
        public float Blue => _blue / 31f;

        /// <summary>
        /// The Alpha of the color, in range [0, 1].
        /// </summary>
        public float Alpha => _alpha / 31f;

        /// <summary>
        /// Controls whether or not the relevant pixel is transparent. If true, the pixel is a semitransparent color,
        /// if false then it's non-transparent.
        /// </summary>
        public bool TransparencyControl => _stp;

        private byte _red => (byte)(_colorData & 0b11111);
        private byte _green => (byte)((_colorData >> 5) & 0b11111);
        private byte _blue => (byte)((_colorData >> 10) & 0b11111);

        private byte _alpha;

        private bool _stp => ((_colorData >> 15) & 0x1) == 1;

        /// <summary>
        /// True if this color is black.
        /// </summary>
        public bool IsBlack => _red == 0 && _green == 0 && _blue == 0;

        private readonly uint _colorData;

        /// <summary>
        /// Read a 16-bit color from the given binary stream.
        /// </summary>
        /// <param name="br">The binary stream.</param>
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
        }

        /// <summary>
        /// Create a 16-bit colour from a short.
        /// </summary>
        /// <param name="data">The short to use.</param>
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
        }

        /// <summary>
        /// Write a 16-bit colour to the given binary stream.
        /// </summary>
        /// <param name="bw">The binary stream.</param>
        public void Write(BinaryWriter bw) { bw.Write((ushort)_colorData); }

        /// <summary>
        /// Checks whether this color is equal to another.
        /// </summary>
        /// <param name="other">The other color.</param>
        /// <returns>True if equal, false otherwise.</returns>
        public bool Equals(Color16Bit other) { return _alpha == other._alpha && _colorData == other._colorData; }

        /// <summary>
        /// Checks whether this color is equal to a given object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>True if equal, false otherwise.</returns>
        public override bool Equals(object obj) { return obj is Color16Bit other && Equals(other); }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_alpha.GetHashCode() * 397) ^ (int)_colorData;
            }
        }
    }
}
