// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber;

using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

/// <summary>
/// Conversions between <see cref="PreciseNumber"/> and the other numeric types, as generic math
/// reaches them through <c>CreateChecked</c>, <c>CreateSaturating</c> and <c>CreateTruncating</c>.
/// </summary>
public readonly partial record struct PreciseNumber
{
	/// <summary>
	/// The widest built-in integer type is 128 bits, and 2^128 has 39 digits, so a value with more
	/// integral digits than this is out of range for every one of them.
	/// </summary>
	private const int MaxPrimitiveIntegerDigits = 39;

	/// <summary>
	/// Integral digits a <see cref="decimal"/> can hold. Its largest value is about 7.9 × 10^28.
	/// </summary>
	private const int MaxDecimalIntegralDigits = 29;

	/// <summary>
	/// The largest power of ten a <see cref="double"/> holds exactly.
	/// </summary>
	private const int MaxExactDoublePowerOfTen = 22;

	/// <summary>
	/// The largest power of ten a <see cref="float"/> holds exactly.
	/// </summary>
	private const int MaxExactSinglePowerOfTen = 10;

	/// <summary>
	/// 2^128, one past the range of every built-in integer type.
	/// </summary>
	private static readonly BigInteger PrimitiveIntegerModulus = BigInteger.One << 128;

	/// <summary>
	/// 2^53, the largest integer below which every integer is exactly representable as a <see cref="double"/>.
	/// </summary>
	private static readonly BigInteger MaxExactDoubleSignificand = BigInteger.One << 53;

	/// <summary>
	/// 2^24, the largest integer below which every integer is exactly representable as a <see cref="float"/>.
	/// </summary>
	private static readonly BigInteger MaxExactSingleSignificand = BigInteger.One << 24;

	private static readonly double[] ExactDoublePowersOfTen =
	[
		1e0, 1e1, 1e2, 1e3, 1e4, 1e5, 1e6, 1e7, 1e8, 1e9, 1e10, 1e11,
		1e12, 1e13, 1e14, 1e15, 1e16, 1e17, 1e18, 1e19, 1e20, 1e21, 1e22,
	];

	private static readonly float[] ExactSinglePowersOfTen =
	[
		1e0f, 1e1f, 1e2f, 1e3f, 1e4f, 1e5f, 1e6f, 1e7f, 1e8f, 1e9f, 1e10f,
	];

	/// <summary>
	/// How a conversion treats a value the destination type cannot hold.
	/// </summary>
	private enum ConversionMode
	{
		Checked,
		Saturating,
		Truncating,
	}

	/// <summary>
	/// Converts a value of another numeric type, throwing when the value has no equivalent.
	/// </summary>
	/// <typeparam name="TOther">The type to convert from.</typeparam>
	/// <param name="value">The value to convert.</param>
	/// <param name="result">The converted value, when the conversion is supported.</param>
	/// <returns>
	/// <c>true</c> if <typeparamref name="TOther"/> is a built-in numeric type or <see cref="BigInteger"/>;
	/// <c>false</c> for any other type.
	/// </returns>
	/// <exception cref="OverflowException"><paramref name="value"/> is NaN or an infinity.</exception>
	/// <remarks>
	/// Integers, <see cref="BigInteger"/> and <see cref="decimal"/> convert exactly. Binary floating point
	/// values convert through their decimal text, so <c>0.3048</c> becomes exactly 0.3048 rather than the
	/// binary fraction nearest to it. A <see cref="double"/> keeps 16 significant digits and a
	/// <see cref="float"/> 8, which is the same rounding <see cref="PreciseNumberExtensions.ToPreciseNumber{TInput}(TInput)"/> applies.
	/// </remarks>
	public static bool TryConvertFromChecked<TOther>(TOther value, out PreciseNumber result)
		where TOther : INumberBase<TOther>
		=> TryConvertFrom(value, ConversionMode.Checked, out result);

	/// <summary>
	/// Converts a value of another numeric type, replacing a value with no equivalent by the nearest one.
	/// </summary>
	/// <typeparam name="TOther">The type to convert from.</typeparam>
	/// <param name="value">The value to convert.</param>
	/// <param name="result">The converted value, when the conversion is supported.</param>
	/// <returns>
	/// <c>true</c> if <typeparamref name="TOther"/> is a built-in numeric type or <see cref="BigInteger"/>;
	/// <c>false</c> for any other type.
	/// </returns>
	/// <exception cref="OverflowException"><paramref name="value"/> is an infinity.</exception>
	/// <remarks>
	/// NaN becomes zero. An infinity still throws, because a type with no largest value has nothing to
	/// saturate to. Both choices match <see cref="BigInteger"/>, the other built-in numeric type with no
	/// NaN and no bound. Every finite value converts exactly as it does for
	/// <see cref="TryConvertFromChecked{TOther}(TOther, out PreciseNumber)"/>.
	/// </remarks>
	public static bool TryConvertFromSaturating<TOther>(TOther value, out PreciseNumber result)
		where TOther : INumberBase<TOther>
		=> TryConvertFrom(value, ConversionMode.Saturating, out result);

	/// <summary>
	/// Converts a value of another numeric type, discarding what the destination cannot represent.
	/// </summary>
	/// <typeparam name="TOther">The type to convert from.</typeparam>
	/// <param name="value">The value to convert.</param>
	/// <param name="result">The converted value, when the conversion is supported.</param>
	/// <returns>
	/// <c>true</c> if <typeparamref name="TOther"/> is a built-in numeric type or <see cref="BigInteger"/>;
	/// <c>false</c> for any other type.
	/// </returns>
	/// <exception cref="OverflowException"><paramref name="value"/> is an infinity.</exception>
	/// <remarks>
	/// Nothing finite needs truncating, since every finite value has an exact equivalent. NaN becomes zero
	/// and an infinity throws, as they do for <see cref="BigInteger"/>.
	/// </remarks>
	public static bool TryConvertFromTruncating<TOther>(TOther value, out PreciseNumber result)
		where TOther : INumberBase<TOther>
		=> TryConvertFrom(value, ConversionMode.Truncating, out result);

	/// <summary>
	/// Converts a <see cref="PreciseNumber"/> to another numeric type, throwing when the value is out of range.
	/// </summary>
	/// <typeparam name="TOther">The type to convert to.</typeparam>
	/// <param name="value">The value to convert.</param>
	/// <param name="result">The converted value, when the conversion is supported.</param>
	/// <returns>
	/// <c>true</c> if <typeparamref name="TOther"/> is a built-in numeric type or <see cref="BigInteger"/>;
	/// <c>false</c> for any other type.
	/// </returns>
	/// <exception cref="OverflowException">
	/// <typeparamref name="TOther"/> is an integer type or <see cref="decimal"/>, and the value is outside its range.
	/// </exception>
	/// <remarks>
	/// Integer types keep the integral part, truncated toward zero. <see cref="double"/>, <see cref="float"/>
	/// and <see cref="Half"/> receive the nearest representable value, correctly rounded however many digits
	/// the number has, and overflow to an infinity rather than throwing, as every built-in conversion to a
	/// binary floating point type does. <see cref="decimal"/> keeps as many digits as it can hold, rounding
	/// the rest the way <see cref="decimal.Parse(string, NumberStyles, IFormatProvider)"/> does.
	/// </remarks>
	public static bool TryConvertToChecked<TOther>(PreciseNumber value, [MaybeNullWhen(false)] out TOther result)
		where TOther : INumberBase<TOther>
		=> TryConvertTo(value, ConversionMode.Checked, out result);

	/// <summary>
	/// Converts a <see cref="PreciseNumber"/> to another numeric type, clamping a value that is out of range.
	/// </summary>
	/// <typeparam name="TOther">The type to convert to.</typeparam>
	/// <param name="value">The value to convert.</param>
	/// <param name="result">The converted value, when the conversion is supported.</param>
	/// <returns>
	/// <c>true</c> if <typeparamref name="TOther"/> is a built-in numeric type or <see cref="BigInteger"/>;
	/// <c>false</c> for any other type.
	/// </returns>
	/// <remarks>
	/// A value beyond the range of an integer type or <see cref="decimal"/> becomes its minimum or maximum.
	/// Everything else converts as it does for <see cref="TryConvertToChecked{TOther}(PreciseNumber, out TOther)"/>.
	/// </remarks>
	public static bool TryConvertToSaturating<TOther>(PreciseNumber value, [MaybeNullWhen(false)] out TOther result)
		where TOther : INumberBase<TOther>
		=> TryConvertTo(value, ConversionMode.Saturating, out result);

	/// <summary>
	/// Converts a <see cref="PreciseNumber"/> to another numeric type, keeping only what the destination can represent.
	/// </summary>
	/// <typeparam name="TOther">The type to convert to.</typeparam>
	/// <param name="value">The value to convert.</param>
	/// <param name="result">The converted value, when the conversion is supported.</param>
	/// <returns>
	/// <c>true</c> if <typeparamref name="TOther"/> is a built-in numeric type or <see cref="BigInteger"/>;
	/// <c>false</c> for any other type.
	/// </returns>
	/// <remarks>
	/// An integer type receives the low bits of the integral part, wrapping exactly as a truncating
	/// conversion from <see cref="BigInteger"/> does. <see cref="decimal"/> clamps instead, which is also
	/// what <see cref="BigInteger"/> does for it. Everything else converts as it does for
	/// <see cref="TryConvertToChecked{TOther}(PreciseNumber, out TOther)"/>.
	/// </remarks>
	public static bool TryConvertToTruncating<TOther>(PreciseNumber value, [MaybeNullWhen(false)] out TOther result)
		where TOther : INumberBase<TOther>
		=> TryConvertTo(value, ConversionMode.Truncating, out result);

	private static bool TryConvertFrom<TOther>(TOther value, ConversionMode mode, out PreciseNumber result)
		where TOther : INumberBase<TOther>
	{
		if (typeof(TOther) == typeof(PreciseNumber))
		{
			result = (PreciseNumber)(object)value;
			return true;
		}

		if (typeof(TOther) == typeof(double))
		{
			return TryConvertFromBinaryFloatingPoint((double)(object)value, mode, out result);
		}

		if (typeof(TOther) == typeof(float))
		{
			return TryConvertFromBinaryFloatingPoint((float)(object)value, mode, out result);
		}

		if (typeof(TOther) == typeof(Half))
		{
			return TryConvertFromBinaryFloatingPoint((Half)(object)value, mode, out result);
		}

		if (typeof(TOther) == typeof(decimal))
		{
			result = CreateFromFloatingPoint((decimal)(object)value);
			return true;
		}

		if (typeof(TOther) == typeof(BigInteger))
		{
			result = new(0, (BigInteger)(object)value);
			return true;
		}

		if (IsPrimitiveInteger<TOther>())
		{
			// Every built-in integer fits in a BigInteger, so this conversion can never fail.
			result = new(0, BigInteger.CreateChecked(value));
			return true;
		}

		result = default;
		return false;
	}

	private static bool TryConvertFromBinaryFloatingPoint<TFloat>(TFloat value, ConversionMode mode, out PreciseNumber result)
		where TFloat : IFloatingPointIeee754<TFloat>
	{
		if (TFloat.IsNaN(value))
		{
			if (mode == ConversionMode.Checked)
			{
				throw new OverflowException("PreciseNumber cannot represent NaN.");
			}

			result = Zero;
			return true;
		}

		if (TFloat.IsInfinity(value))
		{
			throw new OverflowException("PreciseNumber cannot represent infinity.");
		}

		result = CreateFromFloatingPoint(value);
		return true;
	}

	private static bool TryConvertTo<TOther>(PreciseNumber value, ConversionMode mode, [MaybeNullWhen(false)] out TOther result)
		where TOther : INumberBase<TOther>
	{
		if (typeof(TOther) == typeof(PreciseNumber))
		{
			result = (TOther)(object)value;
			return true;
		}

		if (typeof(TOther) == typeof(double))
		{
			result = (TOther)(object)value.ToDouble();
			return true;
		}

		if (typeof(TOther) == typeof(float))
		{
			result = (TOther)(object)value.ToSingle();
			return true;
		}

		if (typeof(TOther) == typeof(Half))
		{
			result = (TOther)(object)value.ParseAs<Half>();
			return true;
		}

		if (typeof(TOther) == typeof(decimal))
		{
			result = (TOther)(object)value.ToDecimal(mode);
			return true;
		}

		if (typeof(TOther) == typeof(BigInteger))
		{
			result = (TOther)(object)value.TruncateToBigInteger();
			return true;
		}

		if (IsPrimitiveInteger<TOther>())
		{
			result = ToPrimitiveInteger<TOther>(value, mode);
			return true;
		}

		result = default;
		return false;
	}

	private static bool IsPrimitiveInteger<T>() =>
		typeof(T) == typeof(int)
		|| typeof(T) == typeof(long)
		|| typeof(T) == typeof(short)
		|| typeof(T) == typeof(sbyte)
		|| typeof(T) == typeof(uint)
		|| typeof(T) == typeof(ulong)
		|| typeof(T) == typeof(ushort)
		|| typeof(T) == typeof(byte)
		|| typeof(T) == typeof(Int128)
		|| typeof(T) == typeof(UInt128)
		|| typeof(T) == typeof(nint)
		|| typeof(T) == typeof(nuint)
		|| typeof(T) == typeof(char);

	/// <summary>
	/// Converts to a built-in integer type by way of <see cref="BigInteger"/>, so that range checks,
	/// clamping and wrapping all follow its conventions exactly.
	/// </summary>
	private static TOther ToPrimitiveInteger<TOther>(PreciseNumber value, ConversionMode mode)
		where TOther : INumberBase<TOther>
	{
		// A large positive exponent would otherwise build a BigInteger with that many digits only to
		// discard nearly all of them.
		if (value.Exponent > 0 && (long)value.Exponent + value.SignificantDigits > MaxPrimitiveIntegerDigits)
		{
			return mode switch
			{
				ConversionMode.Checked => throw new OverflowException($"Value was either too large or too small for {typeof(TOther).Name}."),
				ConversionMode.Saturating => TOther.CreateSaturating(value.Significand.Sign < 0 ? -PrimitiveIntegerModulus : PrimitiveIntegerModulus),
				_ => TOther.CreateTruncating(value.LowIntegerBits()),
			};
		}

		BigInteger integral = value.TruncateToBigInteger();
		return mode switch
		{
			ConversionMode.Checked => TOther.CreateChecked(integral),
			ConversionMode.Saturating => TOther.CreateSaturating(integral),
			_ => TOther.CreateTruncating(integral),
		};
	}

	/// <summary>
	/// Computes the integral value modulo 2^128, which is every bit a truncating conversion to a
	/// built-in integer type keeps, without materializing the full integer.
	/// </summary>
	/// <remarks>Only valid for a non-negative exponent.</remarks>
	private BigInteger LowIntegerBits()
	{
		BigInteger low = BigInteger.Abs(Significand) % PrimitiveIntegerModulus
			* BigInteger.ModPow(Base10, Exponent, PrimitiveIntegerModulus)
			% PrimitiveIntegerModulus;

		return Significand.Sign < 0 && !low.IsZero
			? PrimitiveIntegerModulus - low
			: low;
	}

	/// <summary>
	/// Gets the integral part, truncated toward zero.
	/// </summary>
	private BigInteger TruncateToBigInteger()
	{
		if (Exponent >= 0)
		{
			return Significand * Pow10(Exponent);
		}

		// Every digit sits after the decimal point, so the integral part is zero.
		return -Exponent >= SignificantDigits
			? BigInteger.Zero
			: BigInteger.Divide(Significand, Pow10(-Exponent));
	}

	private double ToDouble()
	{
		// Clinger's fast path: when the significand and the power of ten are both exact doubles, one
		// multiplication or division is correctly rounded by IEEE 754 itself.
		if (int.Abs(Exponent) <= MaxExactDoublePowerOfTen && BigInteger.Abs(Significand) <= MaxExactDoubleSignificand)
		{
			double significand = (double)Significand;
			return Exponent >= 0
				? significand * ExactDoublePowersOfTen[Exponent]
				: significand / ExactDoublePowersOfTen[-Exponent];
		}

		return ParseAs<double>();
	}

	private float ToSingle()
	{
		if (int.Abs(Exponent) <= MaxExactSinglePowerOfTen && BigInteger.Abs(Significand) <= MaxExactSingleSignificand)
		{
			float significand = (float)Significand;
			return Exponent >= 0
				? significand * ExactSinglePowersOfTen[Exponent]
				: significand / ExactSinglePowersOfTen[-Exponent];
		}

		return ParseAs<float>();
	}

	private decimal ToDecimal(ConversionMode mode)
	{
		long integralDigits = (long)Exponent + SignificantDigits;

		// Below 10^28 the value is in range even after rounding its last kept digit up.
		if (Significand.IsZero || integralDigits < MaxDecimalIntegralDigits)
		{
			return ParseAs<decimal>();
		}

		if (integralDigits == MaxDecimalIntegralDigits)
		{
			try
			{
				return ParseAs<decimal>();
			}
			catch (OverflowException) when (mode != ConversionMode.Checked)
			{
				return Significand.Sign < 0 ? decimal.MinValue : decimal.MaxValue;
			}
		}

		return mode == ConversionMode.Checked
			? throw new OverflowException("Value was either too large or too small for a Decimal.")
			: Significand.Sign < 0 ? decimal.MinValue : decimal.MaxValue;
	}

	/// <summary>
	/// Renders the number as <c>significand E exponent</c> and parses it as <typeparamref name="TNumber"/>.
	/// </summary>
	/// <remarks>
	/// The runtime's parsers round correctly however many digits they are given, which is what makes this
	/// exact where multiplying by <c>Math.Pow(10, exponent)</c> is not.
	/// </remarks>
	private TNumber ParseAs<TNumber>()
		where TNumber : INumberBase<TNumber>
	{
		// The significand's digits, its sign, the 'E', and an exponent of up to eleven characters.
		int length = SignificantDigits + 13;
		char[]? rented = length > MaxStackAllocChars ? ArrayPool<char>.Shared.Rent(length) : null;
		Span<char> stackBuffer = stackalloc char[MaxStackAllocChars];
		Span<char> buffer = rented is null ? stackBuffer : rented.AsSpan();

		try
		{
			if (!Significand.TryFormat(buffer, out int written, default, InvariantCulture))
			{
				throw new InvalidOperationException("The significand did not fit the buffer sized for it.");
			}

			buffer[written++] = 'E';

			if (!Exponent.TryFormat(buffer[written..], out int exponentWritten, default, InvariantCulture))
			{
				throw new InvalidOperationException("The exponent did not fit the buffer sized for it.");
			}

			return TNumber.Parse(buffer[..(written + exponentWritten)], NumberStyles.Float, InvariantCulture);
		}
		finally
		{
			if (rented is not null)
			{
				ArrayPool<char>.Shared.Return(rented);
			}
		}
	}
}
