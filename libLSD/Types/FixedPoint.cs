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
    public struct FixedPoint
    {
        public int IntegralPart
        {
            get => (this._value & INTEGRAL_MASK) >> DECIMAL_BITS;
            set => this._value = _value ^ ((_value ^ (value << DECIMAL_BITS)) & INTEGRAL_MASK);
        }

        public int DecimalPart
        {
            get => (this._value & DECIMAL_MASK);
            set => this._value = _value ^ ((_value ^ value) & DECIMAL_MASK);
        }

        public bool IsNegative
        {
            get => (this._value & SIGN_MASK) >> (DECIMAL_BITS + INTEGRAL_BITS) == 1;
            set => this._value = _value ^ ((_value ^ ((value ? 1 : 0) << (DECIMAL_BITS + INTEGRAL_BITS))) & SIGN_MASK);
        }

        public int IntegralAndDecimalPart
        {
            get => (this._value & (DECIMAL_MASK | INTEGRAL_MASK));
        }

        private int _value;

        private const int SIGN_MASK = 0x8000;
        private const int INTEGRAL_MASK = 0x7000;
        private const int DECIMAL_MASK = 0xFFF;
        private const int DECIMAL_BITS = 12;
        private const int INTEGRAL_BITS = 3;
        private const float FIXED_BITVALUE = 1.0f / (1 << DECIMAL_BITS);

        public FixedPoint(int value)
        {
            this._value = value;
        }

        public FixedPoint(int integralPart, int decimalPart, bool isNegative = false)
        {
            _value = 0;
            IntegralPart = integralPart;
            DecimalPart = decimalPart;
            IsNegative = isNegative;
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
            if (p.IsNegative)
            {
                // negative
                //p.IsNegative = false;
                return -FIXED_BITVALUE * -(~p.IntegralAndDecimalPart + 1);
            }
            else
                return FIXED_BITVALUE * p._value;
        }
    }
}
