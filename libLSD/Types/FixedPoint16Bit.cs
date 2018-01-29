using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libLSD.Exceptions;
using libLSD.Util;

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
    public struct FixedPoint16Bit
    {
        public int IntegralPart
        {
            get => (this._value & INTEGRAL_MASK) >> DECIMAL_BITS;
            set => this._value = BitTwiddling.Merge<int>(_value, value, INTEGRAL_MASK, DECIMAL_BITS);
        }

        public int DecimalPart
        {
            get => (this._value & DECIMAL_MASK);
            set => this._value = BitTwiddling.Merge<int>(_value, value, DECIMAL_MASK);
        }

        public bool IsNegative
        {
            get => (this._value & SIGN_MASK) >> (DECIMAL_BITS + INTEGRAL_BITS) == 1;
            set => this._value =
                BitTwiddling.Merge<int>(_value, value ? 1 : 0, SIGN_MASK, DECIMAL_BITS + INTEGRAL_BITS);
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

        public FixedPoint16Bit(int value)
        {
            this._value = value;
        }

        public FixedPoint16Bit(int integralPart, int decimalPart, bool isNegative = false)
        {
            this._value = 0;
            this.IntegralPart = integralPart;
            this.DecimalPart = decimalPart;
            this.IsNegative = isNegative;
        }

        public FixedPoint16Bit(byte[] data)
        {
            if (data.Length != 2)
            {
                throw new BadFormatException("FixedPoint16Bit data must be 2 bytes long!"); }
            if (BitConverter.IsLittleEndian)
                Array.Reverse(data);
            _value = BitConverter.ToUInt16(data, 0);
        }

        public override string ToString()
        {
            return ((float)this).ToString();
        }

        public static implicit operator float(FixedPoint16Bit p)
        {
            if (p.IsNegative)
            {
                return -FIXED_BITVALUE * -(~p.IntegralAndDecimalPart + 1);
            }
            else
                return FIXED_BITVALUE * p._value;
        }

        public static implicit operator FixedPoint16Bit(byte[] bytes) { return new FixedPoint16Bit(bytes); }
    }
}
