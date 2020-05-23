namespace libLSD.Types
{
    /// <summary>
    /// 24-bit representation of a color, as on the PSX. One byte for each of RGB.
    /// </summary>
    public struct Color24Bit : IColor
    {
        /// <summary>
        /// The Red component of the color, in range [0, 1].
        /// </summary>
        public float Red => _red / 255f;

        /// <summary>
        /// The Green component of the color, in range [0, 1].
        /// </summary>
        public float Green => _green / 255f;

        /// <summary>
        /// The Blue component of the color, in range [0, 1].
        /// </summary>
        public float Blue => _blue / 255f;

        /// <summary>
        /// Alpha is unused in a 24-bit color.
        /// </summary>
        public float Alpha { get; }

        /// <summary>
        /// Unused in 24-bit color.
        /// </summary>
        public bool TransparencyControl => false;

        private readonly byte _red;
        private readonly byte _green;
        private readonly byte _blue;

        /// <summary>
        /// True if this color is black.
        /// </summary>
        public bool IsBlack => _red == 0 && _green == 0 && _blue == 0;

        /// <summary>
        /// Create a new 24-bit color from three bytes.
        /// </summary>
        /// <param name="r">Red component.</param>
        /// <param name="g">Green component.</param>
        /// <param name="b">Blue component.</param>
        public Color24Bit(byte r, byte g, byte b)
        {
            _red = r;
            _green = g;
            _blue = b;
            Alpha = 1f;
        }

        public bool Equals(Color24Bit other)
        {
            return _red == other._red && _green == other._green && _blue == other._blue && Alpha.Equals(other.Alpha);
        }

        public override bool Equals(object obj) { return obj is Color24Bit other && Equals(other); }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _red.GetHashCode();
                hashCode = (hashCode * 397) ^ _green.GetHashCode();
                hashCode = (hashCode * 397) ^ _blue.GetHashCode();
                hashCode = (hashCode * 397) ^ Alpha.GetHashCode();
                return hashCode;
            }
        }
    }
}
