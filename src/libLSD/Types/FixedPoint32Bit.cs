﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libLSD.Exceptions;
using libLSD.Interfaces;
using libLSD.Util;

namespace libLSD.Types
{
	/// <summary>
	/// A 32-bit fixed point number as used in PSX hardware.
	/// 
	/// In the form:
	/// <code>
	///     MSB ------------------------------- LSB
	///     SIII IIII IIII IIII IIII MMMM MMMM MMMM
	/// </code>
	/// Where S is sign bit, I are integral bits, and M are mantissa bits
	/// </summary>
	public struct FixedPoint32Bit : IWriteable
	{
		public long IntegralPart
		{
			get => (this._value & INTEGRAL_MASK) >> DECIMAL_BITS;
			set => this._value = BitTwiddling.Merge<long>(_value, value, INTEGRAL_MASK, DECIMAL_BITS);
		}

		public long DecimalPart
		{
			get => (this._value & DECIMAL_MASK);
			set => this._value = BitTwiddling.Merge<long>(_value, value, DECIMAL_MASK);
		}

		public bool IsNegative
		{
			get => (this._value & SIGN_MASK) >> (int)(DECIMAL_BITS + INTEGRAL_BITS) == 1;
			set => this._value =
				BitTwiddling.Merge<long>(_value, value ? 1 : 0, SIGN_MASK, (int)(DECIMAL_BITS + INTEGRAL_BITS));
		}

		public long IntegralAndDecimalPart
		{
			get => (this._value & (DECIMAL_MASK | INTEGRAL_MASK));
		}

		private long _value;

		private const long SIGN_MASK = 0x80000000;
		private const long INTEGRAL_MASK = 0x7FFFF000;
		private const long DECIMAL_MASK = 0xFFF;
		private const int DECIMAL_BITS = 12;
		private const int INTEGRAL_BITS = 19;
		private const float FIXED_BITVALUE = 1.0f / (1 << DECIMAL_BITS);

		public FixedPoint32Bit(uint value)
		{
			this._value = value;
		}

		public FixedPoint32Bit(uint integralPart, uint decimalPart, bool isNegative = false)
		{
			this._value = 0;
			this.IntegralPart = integralPart;
			this.DecimalPart = decimalPart;
			this.IsNegative = isNegative;
		}

		public FixedPoint32Bit(byte[] data)
		{
			if (data.Length != 4)
			{
				throw new BadFormatException("FixedPoint32Bit data must be 4 bytes long!");
			}
			//if (BitConverter.IsLittleEndian)
				//Array.Reverse(data);
			_value = BitConverter.ToInt32(data, 0);
		}

		public override string ToString()
		{
			return ((float)this).ToString();
		}

		public static implicit operator float(FixedPoint32Bit p)
		{
			if (p.IsNegative)
			{
				return -FIXED_BITVALUE * -(~p.IntegralAndDecimalPart + 1);
			}
			else
				return FIXED_BITVALUE * p._value;
		}

		public static implicit operator FixedPoint32Bit(byte[] bytes) { return new FixedPoint32Bit(bytes); }

	    public void Write(BinaryWriter bw)
	    {
	        byte[] bytes = BitConverter.GetBytes((uint)_value);
	        Array.Reverse(bytes);
            bw.Write(bytes);
	    }
	}
}
