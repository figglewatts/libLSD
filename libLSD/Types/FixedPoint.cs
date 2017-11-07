using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libLSD.Types
{
    /// <summary>
    /// A 16-bit fixed point number as used in PSX hardware.
    /// 
    /// In the form:
    /// <code>
    ///     MSB ----------- LSB
    ///     SIII MMMM MMMM MMMM
    /// </code>
    /// Where S is sign bit, I are integral bits, and M are mantissa bits
    /// </summary>
    public class FixedPoint
    {
        public int IntegralPart
        {
            get => (this._value & INTEGRAL_MASK) >> 12;
            set => this._value = _value ^ ((_value ^ (value << 12)) & INTEGRAL_MASK);
        }

        public int DecimalPart
        {
            get => (this._value & MANTISSA_MASK);
            set => this._value = _value ^ ((_value ^ value) & MANTISSA_MASK);
        }

        public bool IsNegative
        {
            get => (this._value & SIGN_MASK) >> 15 == 1;
            set => this._value = _value ^ ((_value ^ ((value ? 1 : 0) << 15)) & SIGN_MASK);
        }

        private int _value;

        const int SIGN_MASK = 0x8000;
        const int INTEGRAL_MASK = 0x7000;
        const int MANTISSA_MASK = 0xFFF;

        public FixedPoint()
        {
            _value = 0;
        }

        public FixedPoint(byte[] data)
        {
            if (data.Length != 2) { throw new ArgumentException("data must be 2 bytes", nameof(data)); }
            if (BitConverter.IsLittleEndian)
                Array.Reverse(data);
            _value = BitConverter.ToUInt16(data, 0);
        }

        public override string ToString()
        {
            return (IsNegative ? "-" : "") + IntegralPart + "." + DecimalPart;
        }

        public static implicit operator float(FixedPoint p)
        {
            return p._value / 65536.0f;
        }
    }
}
