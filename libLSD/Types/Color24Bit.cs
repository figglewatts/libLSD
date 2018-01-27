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

        private readonly byte _red;
        private readonly byte _green;
        private readonly byte _blue;

        public Color24Bit(byte r, byte g, byte b)
        {
            _red = r;
            _green = g;
            _blue = b;
            Alpha = 1f;
        }
    }
}
