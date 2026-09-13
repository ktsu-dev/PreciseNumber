// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber.Test;

using System.Globalization;
using System.Numerics;

/// <summary>
/// Covers conversion through generic math, which is how code written against <see cref="INumber{TSelf}"/>
/// reaches <see cref="PreciseNumber"/>.
/// </summary>
[TestClass]
public class PreciseNumberConversionTests
{
	private static PreciseNumber P(string text) => PreciseNumber.Parse(text, CultureInfo.InvariantCulture);

	private static TTo Checked<TTo, TFrom>(TFrom value)
		where TTo : INumberBase<TTo>
		where TFrom : INumberBase<TFrom>
		=> TTo.CreateChecked(value);

	private static TTo Saturating<TTo, TFrom>(TFrom value)
		where TTo : INumberBase<TTo>
		where TFrom : INumberBase<TFrom>
		=> TTo.CreateSaturating(value);

	private static TTo Truncating<TTo, TFrom>(TFrom value)
		where TTo : INumberBase<TTo>
		where TFrom : INumberBase<TFrom>
		=> TTo.CreateTruncating(value);

	/// <summary>The shape of a unit conversion in generic quantity code.</summary>
	private static T ToBase<T>(T value, double factor)
		where T : INumber<T>
		=> value * T.CreateChecked(factor);

	private static void AssertFromInAllModes<TFrom>(TFrom value, string expected)
		where TFrom : INumberBase<TFrom>
	{
		PreciseNumber expectedNumber = P(expected);
		Assert.AreEqual(expectedNumber, Checked<PreciseNumber, TFrom>(value), $"Checked from {typeof(TFrom).Name}");
		Assert.AreEqual(expectedNumber, Saturating<PreciseNumber, TFrom>(value), $"Saturating from {typeof(TFrom).Name}");
		Assert.AreEqual(expectedNumber, Truncating<PreciseNumber, TFrom>(value), $"Truncating from {typeof(TFrom).Name}");
	}

	private static void AssertToInAllModes<TTo>(string value, TTo expected)
		where TTo : INumberBase<TTo>
	{
		PreciseNumber number = P(value);
		Assert.AreEqual(expected, Checked<TTo, PreciseNumber>(number), $"Checked to {typeof(TTo).Name}");
		Assert.AreEqual(expected, Saturating<TTo, PreciseNumber>(number), $"Saturating to {typeof(TTo).Name}");
		Assert.AreEqual(expected, Truncating<TTo, PreciseNumber>(number), $"Truncating to {typeof(TTo).Name}");
	}

	[TestMethod]
	public void FromEveryIntegerTypeIsExact()
	{
		AssertFromInAllModes(sbyte.MinValue, "-128");
		AssertFromInAllModes(byte.MaxValue, "255");
		AssertFromInAllModes(short.MinValue, "-32768");
		AssertFromInAllModes(ushort.MaxValue, "65535");
		AssertFromInAllModes(int.MinValue, "-2147483648");
		AssertFromInAllModes(uint.MaxValue, "4294967295");
		AssertFromInAllModes(long.MinValue, "-9223372036854775808");
		AssertFromInAllModes(ulong.MaxValue, "18446744073709551615");
		AssertFromInAllModes(Int128.MinValue, "-170141183460469231731687303715884105728");
		AssertFromInAllModes(UInt128.MaxValue, "340282366920938463463374607431768211455");
		AssertFromInAllModes((nint)(-42), "-42");
		AssertFromInAllModes((nuint)42, "42");
		AssertFromInAllModes('A', "65");
		AssertFromInAllModes(BigInteger.Pow(10, 60) + 1, "1" + new string('0', 59) + "1");
		AssertFromInAllModes(1000, "1000");
	}

	[TestMethod]
	public void FromDecimalIsExact()
	{
		AssertFromInAllModes(1234.5678m, "1234.5678");
		AssertFromInAllModes(decimal.MaxValue, "79228162514264337593543950335");
		AssertFromInAllModes(decimal.MinValue, "-79228162514264337593543950335");
		AssertFromInAllModes(0.0000000000000000000000000001m, "0.0000000000000000000000000001");
	}

	[TestMethod]
	public void FromBinaryFloatingPointUsesDecimalText()
	{
		AssertFromInAllModes(0.3048, "0.3048");
		AssertFromInAllModes(-1.5e-10, "-0.00000000015");
		AssertFromInAllModes(1.25f, "1.25");
		AssertFromInAllModes((Half)1.5, "1.5");
		AssertFromInAllModes(0.0, "0");
	}

	[TestMethod]
	public void FromPreciseNumberIsIdentity()
	{
		AssertFromInAllModes(P("-12.5"), "-12.5");
	}

	[TestMethod]
	public void GenericUnitConversionIsExact()
	{
		Assert.AreEqual(P("0.3048"), ToBase(PreciseNumber.One, 0.3048));
		Assert.AreEqual(P("3.048"), ToBase(10.ToPreciseNumber(), 0.3048));
	}

	[TestMethod]
	public void FromNaNMatchesBigInteger()
	{
		// BigInteger is the other built-in numeric type with no NaN and no bound, so it sets the convention.
		Assert.ThrowsExactly<OverflowException>(() => Checked<BigInteger, double>(double.NaN));
		Assert.AreEqual(BigInteger.Zero, Saturating<BigInteger, double>(double.NaN));
		Assert.AreEqual(BigInteger.Zero, Truncating<BigInteger, double>(double.NaN));

		Assert.ThrowsExactly<OverflowException>(() => Checked<PreciseNumber, double>(double.NaN));
		Assert.AreEqual(PreciseNumber.Zero, Saturating<PreciseNumber, double>(double.NaN));
		Assert.AreEqual(PreciseNumber.Zero, Truncating<PreciseNumber, float>(float.NaN));
		Assert.AreEqual(PreciseNumber.Zero, Saturating<PreciseNumber, Half>(Half.NaN));
	}

	[TestMethod]
	public void FromInfinityMatchesBigInteger()
	{
		Assert.ThrowsExactly<OverflowException>(() => Checked<BigInteger, double>(double.PositiveInfinity));
		Assert.ThrowsExactly<OverflowException>(() => Saturating<BigInteger, double>(double.PositiveInfinity));
		Assert.ThrowsExactly<OverflowException>(() => Truncating<BigInteger, double>(double.NegativeInfinity));

		Assert.ThrowsExactly<OverflowException>(() => Checked<PreciseNumber, double>(double.PositiveInfinity));
		Assert.ThrowsExactly<OverflowException>(() => Saturating<PreciseNumber, double>(double.PositiveInfinity));
		Assert.ThrowsExactly<OverflowException>(() => Truncating<PreciseNumber, float>(float.NegativeInfinity));
	}

	[TestMethod]
	public void TryConvertReportsUnsupportedTypes()
	{
		Assert.IsFalse(PreciseNumber.TryConvertFromChecked(new Complex(1, 0), out PreciseNumber _), "Complex is not a supported source");
		Assert.IsFalse(PreciseNumber.TryConvertToChecked(PreciseNumber.One, out Complex _), "Complex is not a supported destination");
	}

	[TestMethod]
	public void ToIntegerTruncatesTowardZero()
	{
		AssertToInAllModes("12.9", 12);
		AssertToInAllModes("-12.9", -12);
		AssertToInAllModes("0.999", 0L);
		AssertToInAllModes("-0.5", (short)0);
		AssertToInAllModes("12345e3", 12345000);
		AssertToInAllModes("65.7", 'A');
		AssertToInAllModes("-170141183460469231731687303715884105728", Int128.MinValue);
		AssertToInAllModes("340282366920938463463374607431768211455", UInt128.MaxValue);
		AssertToInAllModes("-42.1", (nint)(-42));
		AssertToInAllModes("42.1", (nuint)42);
		AssertToInAllModes("255", byte.MaxValue);
		AssertToInAllModes("-128", sbyte.MinValue);
		AssertToInAllModes("65535", ushort.MaxValue);
		AssertToInAllModes("4294967295", uint.MaxValue);
		AssertToInAllModes("18446744073709551615.5", ulong.MaxValue);
	}

	[TestMethod]
	public void ToIntegerOutOfRange()
	{
		PreciseNumber tooLargeForByte = P("300");
		Assert.ThrowsExactly<OverflowException>(() => Checked<byte, PreciseNumber>(tooLargeForByte));
		Assert.AreEqual(byte.MaxValue, Saturating<byte, PreciseNumber>(tooLargeForByte));
		Assert.AreEqual((byte)44, Truncating<byte, PreciseNumber>(tooLargeForByte));

		PreciseNumber negative = P("-1");
		Assert.ThrowsExactly<OverflowException>(() => Checked<uint, PreciseNumber>(negative));
		Assert.AreEqual(uint.MinValue, Saturating<uint, PreciseNumber>(negative));
		Assert.AreEqual(uint.MaxValue, Truncating<uint, PreciseNumber>(negative));

		PreciseNumber pastInt = P("2147483648");
		Assert.ThrowsExactly<OverflowException>(() => Checked<int, PreciseNumber>(pastInt));
		Assert.AreEqual(int.MaxValue, Saturating<int, PreciseNumber>(pastInt));
		Assert.AreEqual(int.MinValue, Truncating<int, PreciseNumber>(pastInt));

		Assert.ThrowsExactly<OverflowException>(() => Checked<char, PreciseNumber>(P("70000")));
	}

	[TestMethod]
	public void ToIntegerFromHugeValuesMatchesBigInteger()
	{
		foreach (string text in new[] { "7e5000", "-7e5000", "123456789e50", "-98765e45", "3e39", "1e38" })
		{
			PreciseNumber number = P(text);
			BigInteger integer = number.To<BigInteger>();

			Assert.AreEqual(Truncating<int, BigInteger>(integer), Truncating<int, PreciseNumber>(number), $"int truncating {text}");
			Assert.AreEqual(Truncating<ulong, BigInteger>(integer), Truncating<ulong, PreciseNumber>(number), $"ulong truncating {text}");
			Assert.AreEqual(Truncating<Int128, BigInteger>(integer), Truncating<Int128, PreciseNumber>(number), $"Int128 truncating {text}");
			Assert.AreEqual(Truncating<UInt128, BigInteger>(integer), Truncating<UInt128, PreciseNumber>(number), $"UInt128 truncating {text}");
			Assert.AreEqual(Saturating<long, BigInteger>(integer), Saturating<long, PreciseNumber>(number), $"long saturating {text}");
			Assert.AreEqual(Saturating<UInt128, BigInteger>(integer), Saturating<UInt128, PreciseNumber>(number), $"UInt128 saturating {text}");
		}

		Assert.ThrowsExactly<OverflowException>(() => Checked<Int128, PreciseNumber>(P("7e5000")));
	}

	[TestMethod]
	public void ToBigIntegerTruncatesTowardZero()
	{
		AssertToInAllModes("12345e5", BigInteger.Parse("1234500000", CultureInfo.InvariantCulture));
		AssertToInAllModes("-12.9", new BigInteger(-12));
		AssertToInAllModes("0.0001", BigInteger.Zero);
	}

	[TestMethod]
	public void ToDoubleIsCorrectlyRounded()
	{
		string[] texts =
		[
			"0.1",
			"0.3048",
			"-123.456",
			"3.14159265358979323846264338327950288419716939937510582097494",
			"9007199254740993",
			"9007199254740995",
			"2.2250738585072011e-308",
			"4.9406564584124654e-324",
			"1.7976931348623157e308",
			"123456789012345678901234567890e-45",
			"0.000000000000000000000000000000000000000001",
		];

		foreach (string text in texts)
		{
			double expected = double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
			AssertToInAllModes(text, expected);
		}
	}

	[TestMethod]
	public void ToDoubleOverflowsToInfinity()
	{
		AssertToInAllModes("1e400", double.PositiveInfinity);
		AssertToInAllModes("-1e400", double.NegativeInfinity);
		AssertToInAllModes("1e-400", 0.0);
	}

	[TestMethod]
	public void ToSingleAndHalfAreCorrectlyRounded()
	{
		foreach (string text in new[] { "0.1", "16777217", "3.4028235e38", "1.401298464324817e-45", "2.718281828459045235360287471352662497757" })
		{
			AssertToInAllModes(text, float.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture));
		}

		foreach (string text in new[] { "0.1", "65504", "65520", "1.5", "-3.14159" })
		{
			AssertToInAllModes(text, Half.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture));
		}
	}

	[TestMethod]
	public void ToDecimalRoundsExcessDigits()
	{
		AssertToInAllModes("1234.5678", 1234.5678m);
		AssertToInAllModes("79228162514264337593543950335", decimal.MaxValue);

		const string longFraction = "0.12345678901234567890123456789012345678901234";
		AssertToInAllModes(longFraction, decimal.Parse(longFraction, NumberStyles.Float, CultureInfo.InvariantCulture));
	}

	[TestMethod]
	public void ToDecimalOutOfRange()
	{
		foreach (string text in new[] { "1e30", "79228162514264337593543950336" })
		{
			PreciseNumber number = P(text);
			Assert.ThrowsExactly<OverflowException>(() => Checked<decimal, PreciseNumber>(number), text);
			Assert.AreEqual(decimal.MaxValue, Saturating<decimal, PreciseNumber>(number), text);
			Assert.AreEqual(decimal.MaxValue, Truncating<decimal, PreciseNumber>(number), text);
			Assert.AreEqual(decimal.MinValue, Saturating<decimal, PreciseNumber>(-number), text);
		}

		// BigInteger clamps rather than wraps when truncating to decimal, and so does PreciseNumber.
		Assert.AreEqual(decimal.MaxValue, Truncating<decimal, BigInteger>(BigInteger.Pow(10, 30)));
	}

	[TestMethod]
	public void BuiltInCreateCheckedReachesPreciseNumber()
	{
		Assert.AreEqual(1.5, double.CreateChecked(P("1.5")));
		Assert.AreEqual(12, int.CreateChecked(P("12.75")));
		Assert.AreEqual(1234.5678m, decimal.CreateSaturating(P("1234.5678")));
		Assert.AreEqual(BigInteger.One, BigInteger.CreateTruncating(P("1.9")));
	}

	[TestMethod]
	public void ToUsesTheSameConversions()
	{
		Assert.AreEqual(12, P("12.9").To<int>());
		Assert.AreEqual(
			double.Parse("3.14159265358979323846264338327950288419716939937510582097494", NumberStyles.Float, CultureInfo.InvariantCulture),
			P("3.14159265358979323846264338327950288419716939937510582097494").To<double>());
	}
}
