// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber.Test;

using System.Globalization;
using System.Numerics;

/// <summary>
/// Pins how Parse applies a written exponent at the edges of the <see cref="int"/> exponent range:
/// padding zeros must not overflow a value that fits, and a zero is zero whatever its exponent.
/// </summary>
[TestClass]
public sealed class PreciseNumberParseExponentTests
{
	[TestMethod]
	[DataRow("1e-2147483646", -2147483646, 1)]
	[DataRow("1.000000E-2147483646", -2147483646, 1)]
	[DataRow("2.500E-2147483647", -2147483648, 25)]
	[DataRow("1.0e-2147483648", -2147483648, 1)]
	[DataRow("-1.50e-2147483647", -2147483648, -15)]
	[DataRow("10e2147483646", 2147483647, 1)]
	[DataRow("1000e2147483644", 2147483647, 1)]
	public void Parse_AppliesTheExponentAfterDroppingPaddingZeros(string text, int exponent, int significand)
	{
		PreciseNumber expected = new(exponent, new BigInteger(significand));

		Assert.AreEqual(expected, PreciseNumber.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture));
		Assert.IsTrue(PreciseNumber.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out PreciseNumber parsed));
		Assert.AreEqual(expected, parsed);
	}

	[TestMethod]
	[DataRow("0e99999999999")]
	[DataRow("0e-99999999999")]
	[DataRow("0.000e99999999999999999999999")]
	[DataRow("-0e2147483648")]
	[DataRow("000e5")]
	public void Parse_ReadsZeroWhateverItsWrittenExponent(string text)
	{
		Assert.AreEqual(0m, decimal.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture));
		Assert.AreEqual(PreciseNumber.Zero, PreciseNumber.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture));
	}

	[TestMethod]
	[DataRow("1e2147483648")]
	[DataRow("1e-2147483649")]
	[DataRow("0.1e-2147483648")]
	[DataRow("1e99999999999999999999")]
	public void Parse_StillOverflowsForAValueOutsideTheExponentRange(string text)
	{
		Assert.ThrowsExactly<OverflowException>(() => PreciseNumber.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture));
		Assert.IsFalse(PreciseNumber.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out _));
	}

	[TestMethod]
	[DataRow("0e")]
	[DataRow("0ex")]
	[DataRow("0e 5")]
	public void Parse_RejectsAMalformedExponentEvenOnZero(string text) =>
		Assert.ThrowsExactly<FormatException>(() => PreciseNumber.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture));

	[TestMethod]
	public void Parse_RoundTripsScientificTextAtTheEdgesOfTheExponentRange()
	{
		// At most seven digits, which is all the default "E" format writes.
		BigInteger[] significands = [BigInteger.One, new BigInteger(25), new BigInteger(-1234567)];
		for (int offset = 0; offset <= 10; offset++)
		{
			foreach (BigInteger significand in significands)
			{
				foreach (int exponent in new[] { int.MinValue + offset, int.MaxValue - offset })
				{
					PreciseNumber value = new(exponent, significand);
					string text = value.ToString("E", CultureInfo.InvariantCulture);

					Assert.IsTrue(
						PreciseNumber.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out PreciseNumber parsed),
						$"\"{text}\" did not parse.");
					Assert.AreEqual(value, parsed, $"\"{text}\" parsed to a different value.");
				}
			}
		}
	}
}
