using System;
using System.IO;
using libLSD.Exceptions;
using libLSD.Interfaces;
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
    /// Where S is sign bit, I are integral bits, and M are mantissa bits.
    /// </summary>
    public struct FixedPoint16Bit : IWriteable
    {
        /// <summary>
        /// The integral part of the number.
        /// </summary>
        public int IntegralPart
        {
            get => (this._value & INTEGRAL_MASK) >> DECIMAL_BITS;
            set => this._value = BitTwiddling.Merge(_value, value, INTEGRAL_MASK, DECIMAL_BITS);
        }

        /// <summary>
        /// The decimal part of the number.
        /// </summary>
        public int DecimalPart
        {
            get => (this._value & DECIMAL_MASK);
            set => this._value = BitTwiddling.Merge(_value, value, DECIMAL_MASK);
        }

        /// <summary>
        /// True if this number is negative, false otherwise.
        /// </summary>
        public bool IsNegative
        {
            get => (this._value & SIGN_MASK) >> (DECIMAL_BITS + INTEGRAL_BITS) == 1;
            set =>
                this._value =
                    BitTwiddling.Merge(_value, value ? 1 : 0, SIGN_MASK, DECIMAL_BITS + INTEGRAL_BITS);
        }

        /// <summary>
        /// Gets the integral and decimal parts of the number together.
        /// </summary>
        public int IntegralAndDecimalPart { get => (this._value & (DECIMAL_MASK | INTEGRAL_MASK)); }

        private int _value;

        private const int SIGN_MASK = 0x8000;
        private const int INTEGRAL_MASK = 0x7000;
        private const int DECIMAL_MASK = 0xFFF;
        private const int DECIMAL_BITS = 12;
        private const int INTEGRAL_BITS = 3;
        private const float FIXED_BITVALUE = 1.0f / (1 << DECIMAL_BITS);

        /// <summary>
        /// Create a 16-bit fixed point number from a given value.
        /// </summary>
        /// <param name="value">The value.</param>
        public FixedPoint16Bit(int value) { this._value = value; }

        /// <summary>
        /// Create a 16-bit fixed point number from its constituent parts.
        /// </summary>
        /// <param name="integralPart">The integral part.</param>
        /// <param name="decimalPart">The decimal part.</param>
        /// <param name="isNegative">Whether this number is negative.</param>
        public FixedPoint16Bit(int integralPart, int decimalPart, bool isNegative = false)
        {
            this._value = 0;
            this.IntegralPart = integralPart;
            this.DecimalPart = decimalPart;
            this.IsNegative = isNegative;
        }

        /// <summary>
        /// Create a 16-bit fixed point number from a byte array.
        /// </summary>
        /// <param name="data">The byte array.</param>
        /// <exception cref="BadFormatException">If the byte array is not 2 bytes long.</exception>
        public FixedPoint16Bit(byte[] data)
        {
            if (data.Length != 2)
            {
                throw new BadFormatException("FixedPoint16Bit data must be 2 bytes long!");
            }

            if (BitConverter.IsLittleEndian)
                Array.Reverse(data);
            _value = BitConverter.ToUInt16(data, 0);
        }

        public override string ToString() { return ((float)this).ToString(); }

        /// <summary>
        /// Convert this 16-bit fixed point number to a float.
        /// </summary>
        /// <param name="p">The number.</param>
        /// <returns>The float.</returns>
        public static implicit operator float(FixedPoint16Bit p)
        {
            if (p.IsNegative)
            {
                return -FIXED_BITVALUE * -(~p.IntegralAndDecimalPart + 1);
            }
            else
                return FIXED_BITVALUE * p._value;
        }

        /// <summary>
        /// Convert an array of bytes into a 16-bit fixed point number.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>The fixed point number.</returns>
        public static implicit operator FixedPoint16Bit(byte[] bytes) { return new FixedPoint16Bit(bytes); }

        /// <summary>
        /// Write this 16-bit fixed point number to a binary stream.
        /// </summary>
        /// <param name="bw">The stream.</param>
        public void Write(BinaryWriter bw)
        {
            byte[] bytes = BitConverter.GetBytes((ushort)_value);
            Array.Reverse(bytes);
            bw.Write(bytes);
        }
    }
}
