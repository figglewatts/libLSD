using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libLSD.Types
{
    public struct Color24Bit : IColor
    {
        public float Red => _red / 255f;
        public float Green => _green / 255f;
        public float Blue => _blue / 255f;
        public float Alpha { get; }
        public bool TransparencyControl => false;

        private readonly byte _red;
        private readonly byte _green;
        private readonly byte _blue;

        public bool IsBlack => _red == 0 && _green == 0 && _blue == 0;

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

        public override bool Equals(object obj)
        {
            return obj is Color24Bit other && Equals(other);
        }

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
