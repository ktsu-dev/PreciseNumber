// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber.Test;

using System.Globalization;
using System.Numerics;

/// <summary>
/// Pins the mathematical constants against independent computations of the same values.
/// </summary>
/// <remarks>
/// Every constant is checked digit for digit against a series that shares nothing with the literal
/// in the source, so an edit that corrupts a literal cannot pass. Each series is evaluated in
/// <see cref="BigInteger"/> fixed point with guard digits, then rounded the same way the literal
/// was.
/// </remarks>
[TestClass]
public class PreciseNumberConstantTests
{
	/// <summary>
	/// Digits carried beyond <see cref="PreciseNumber.ConstantPrecision"/> while a series is summed,
	/// so that truncation in the guard region cannot reach the digits under test.
	/// </summary>
	private const int GuardDigits = 40;

	/// <summary>
	/// The fixed point scale every series below is evaluated in. A value <c>v</c> is held as
	/// <c>round(v * Scale)</c>.
	/// </summary>
	private static readonly BigInteger Scale = BigInteger.Pow(10, PreciseNumber.ConstantPrecision + GuardDigits);

	[TestMethod]
	public void TestPiMatchesMachinsFormula()
	{
		// pi = 16*arctan(1/5) - 4*arctan(1/239)
		BigInteger pi = (16 * ArctanReciprocal(5)) - (4 * ArctanReciprocal(239));
		PreciseNumber expected = RoundToSignificantDigits(pi, PreciseNumber.ConstantPrecision);

		Assert.AreEqual(expected, PreciseNumber.Pi, "Pi does not match Machin's formula");
		Assert.AreEqual(expected.Significand, PreciseNumber.Pi.Significand);
		Assert.AreEqual(expected.Exponent, PreciseNumber.Pi.Exponent);
		Assert.AreEqual(PreciseNumber.ConstantPrecision, PreciseNumber.Pi.SignificantDigits);
	}

	[TestMethod]
	public void TestPiIsRoundedRatherThanTruncated()
	{
		// pi = 3.14159265358979323846264338327950288..., so a 26 digit pi ends 434 when it is
		// rounded and 433 when it is truncated. The literal used to end 433.
		PreciseNumber pi26 = PreciseNumber.PiTo(26);

		Assert.AreEqual(
			"31415926535897932384626434",
			pi26.Significand.ToString(CultureInfo.InvariantCulture),
			"Pi truncates where it should round");

		// The same at full precision: the 151st significant digit of pi is 8, so the 150th rounds
		// up from 2 to 3.
		string digits = PreciseNumber.Pi.Significand.ToString(CultureInfo.InvariantCulture);
		Assert.AreEqual('3', digits[^1], "The last digit of Pi is not rounded up");
	}

	[TestMethod]
	public void TestTauMatchesMachinsFormulaDoubled()
	{
		BigInteger pi = (16 * ArctanReciprocal(5)) - (4 * ArctanReciprocal(239));
		PreciseNumber expected = RoundToSignificantDigits(2 * pi, PreciseNumber.ConstantPrecision);

		Assert.AreEqual(expected, PreciseNumber.Tau, "Tau does not match Machin's formula doubled");
		Assert.AreEqual(expected.Significand, PreciseNumber.Tau.Significand);
		Assert.AreEqual(expected.Exponent, PreciseNumber.Tau.Exponent);
		Assert.AreEqual(PreciseNumber.ConstantPrecision, PreciseNumber.Tau.SignificantDigits);
	}

	[TestMethod]
	public void TestTauIsExactlyPiDoubled()
	{
		// The two are independent literals. Nothing but this assertion stops them drifting apart.
		Assert.AreEqual(PreciseNumber.Pi * 2.ToPreciseNumber(), PreciseNumber.Tau);
	}

	[TestMethod]
	public void TestEMatchesItsSeries()
	{
		// e = sum 1/k!
		BigInteger e = Scale;
		BigInteger term = Scale;
		for (int k = 1; term != BigInteger.Zero; k++)
		{
			term /= k;
			e += term;
		}

		PreciseNumber expected = RoundToSignificantDigits(e, PreciseNumber.ConstantPrecision);

		Assert.AreEqual(expected, PreciseNumber.E, "E does not match its series");
		Assert.AreEqual(expected.Significand, PreciseNumber.E.Significand);
		Assert.AreEqual(expected.Exponent, PreciseNumber.E.Exponent);
		Assert.AreEqual(PreciseNumber.ConstantPrecision, PreciseNumber.E.SignificantDigits);
	}

	[TestMethod]
	public void TestLn2MatchesItsSeries()
	{
		// ln(2) = 2*artanh(1/3)
		PreciseNumber expected = RoundToSignificantDigits(2 * ArtanhReciprocal(3), PreciseNumber.ConstantPrecision);

		Assert.AreEqual(expected, PreciseNumber.Ln2, "Ln2 does not match its series");
		Assert.AreEqual(expected.Significand, PreciseNumber.Ln2.Significand);
		Assert.AreEqual(expected.Exponent, PreciseNumber.Ln2.Exponent);
		Assert.AreEqual(PreciseNumber.ConstantPrecision, PreciseNumber.Ln2.SignificantDigits);
	}

	[TestMethod]
	public void TestLn10MatchesItsSeries()
	{
		// ln(10) = ln(8) + ln(10/8) = 6*artanh(1/3) + 2*artanh(1/9)
		PreciseNumber expected = RoundToSignificantDigits(
			(6 * ArtanhReciprocal(3)) + (2 * ArtanhReciprocal(9)),
			PreciseNumber.ConstantPrecision);

		Assert.AreEqual(expected, PreciseNumber.Ln10, "Ln10 does not match its series");
		Assert.AreEqual(expected.Significand, PreciseNumber.Ln10.Significand);
		Assert.AreEqual(expected.Exponent, PreciseNumber.Ln10.Exponent);

		// The 150th significant digit of ln(10) is a zero, which the constructor strips along with
		// any other trailing zero. The value is still correct to 150 digits.
		Assert.AreEqual(PreciseNumber.ConstantPrecision - 1, PreciseNumber.Ln10.SignificantDigits);
	}

	[TestMethod]
	public void TestConstantsCarryAtLeastMinimumDivisionPrecision()
	{
		// A constant shorter than this caps any expression that mixes it with a quotient, silently.
		foreach (PreciseNumber constant in new[]
		{
			PreciseNumber.E,
			PreciseNumber.Pi,
			PreciseNumber.Tau,
			PreciseNumber.Ln2,
			PreciseNumber.Ln10,
		})
		{
			Assert.IsGreaterThanOrEqualTo(
				PreciseNumber.MinimumDivisionPrecision,
				constant.SignificantDigits,
				$"a constant carries only {constant.SignificantDigits} significant digits");
		}
	}

	[TestMethod]
	public void TestQuotientOfPiIsCorrectPastTheOldPrecision()
	{
		// pi/3 to 50 digits needs a pi of at least 50 digits. The 26 digit literal could not do it.
		BigInteger pi = (16 * ArctanReciprocal(5)) - (4 * ArctanReciprocal(239));
		PreciseNumber expected = RoundToSignificantDigits(pi / 3, PreciseNumber.MinimumDivisionPrecision);

		PreciseNumber actual = (PreciseNumber.Pi / 3.ToPreciseNumber())
			.ReduceSignificance(PreciseNumber.MinimumDivisionPrecision);

		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	public void TestPiToReducesToTheRequestedPrecision()
	{
		BigInteger pi = (16 * ArctanReciprocal(5)) - (4 * ArctanReciprocal(239));

		for (int digits = 1; digits <= PreciseNumber.ConstantPrecision; digits++)
		{
			PreciseNumber expected = RoundToSignificantDigits(pi, digits);
			Assert.AreEqual(expected, PreciseNumber.PiTo(digits), $"PiTo({digits}) is not correctly rounded");
		}
	}

	[TestMethod]
	public void TestConstantAccessorsReduceToTheRequestedPrecision()
	{
		Assert.AreEqual(PreciseNumber.E.ReduceSignificance(20), PreciseNumber.ETo(20));
		Assert.AreEqual(PreciseNumber.Pi.ReduceSignificance(20), PreciseNumber.PiTo(20));
		Assert.AreEqual(PreciseNumber.Tau.ReduceSignificance(20), PreciseNumber.TauTo(20));
		Assert.AreEqual(PreciseNumber.Ln2.ReduceSignificance(20), PreciseNumber.Ln2To(20));
		Assert.AreEqual(PreciseNumber.Ln10.ReduceSignificance(20), PreciseNumber.Ln10To(20));

		Assert.AreEqual(20, PreciseNumber.PiTo(20).SignificantDigits);
	}

	[TestMethod]
	public void TestConstantAccessorsServeRepeatedRequestsIdentically()
	{
		// The second call comes from the cache; it has to be the same number as the first.
		Assert.AreEqual(PreciseNumber.PiTo(30), PreciseNumber.PiTo(30));
		Assert.AreEqual(PreciseNumber.PiTo(30).Significand, PreciseNumber.PiTo(30).Significand);
		Assert.AreEqual(PreciseNumber.PiTo(30).Exponent, PreciseNumber.PiTo(30).Exponent);

		// Two precisions of the same constant must not collide in that cache.
		Assert.AreNotEqual(PreciseNumber.PiTo(30), PreciseNumber.PiTo(20));

		// Nor may two constants asked for the same precision.
		Assert.AreNotEqual(PreciseNumber.PiTo(30), PreciseNumber.TauTo(30));
	}

	[TestMethod]
	public void TestConstantAccessorsReturnTheWholeConstantWhenAskedForMore()
	{
		Assert.AreEqual(PreciseNumber.Pi, PreciseNumber.PiTo(PreciseNumber.ConstantPrecision));
		Assert.AreEqual(PreciseNumber.Pi, PreciseNumber.PiTo(PreciseNumber.ConstantPrecision + 100));
		Assert.AreEqual(PreciseNumber.E, PreciseNumber.ETo(int.MaxValue));
		Assert.AreEqual(PreciseNumber.Tau, PreciseNumber.TauTo(int.MaxValue));
		Assert.AreEqual(PreciseNumber.Ln2, PreciseNumber.Ln2To(int.MaxValue));
		Assert.AreEqual(PreciseNumber.Ln10, PreciseNumber.Ln10To(int.MaxValue));
	}

	[TestMethod]
	public void TestConstantAccessorsRejectFewerThanOneDigit()
	{
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.ETo(0));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.PiTo(0));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.TauTo(-1));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.Ln2To(-1));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.Ln10To(int.MinValue));
	}

	/// <summary>
	/// Sums <c>arctan(1/n) = sum (-1)^k / ((2k+1) n^(2k+1))</c> in fixed point.
	/// </summary>
	/// <param name="n">The reciprocal of the argument.</param>
	/// <returns><c>arctan(1/n) * Scale</c>.</returns>
	private static BigInteger ArctanReciprocal(int n)
	{
		BigInteger total = Scale / n;
		BigInteger term = total;
		BigInteger squared = (BigInteger)n * n;
		int k = 1;

		while (term != BigInteger.Zero)
		{
			term /= squared;
			k += 2;
			total += k % 4 == 3 ? -(term / k) : term / k;
		}

		return total;
	}

	/// <summary>
	/// Sums <c>artanh(1/n) = sum 1 / ((2k+1) n^(2k+1))</c> in fixed point.
	/// </summary>
	/// <param name="n">The reciprocal of the argument.</param>
	/// <returns><c>artanh(1/n) * Scale</c>.</returns>
	private static BigInteger ArtanhReciprocal(int n)
	{
		BigInteger total = Scale / n;
		BigInteger term = total;
		BigInteger squared = (BigInteger)n * n;
		int k = 1;

		while (term != BigInteger.Zero)
		{
			term /= squared;
			k += 2;
			total += term / k;
		}

		return total;
	}

	/// <summary>
	/// Rounds a fixed point value to a number of significant digits, half away from zero.
	/// </summary>
	/// <param name="value">The value, scaled by <see cref="Scale"/>.</param>
	/// <param name="significantDigits">The number of significant digits to keep.</param>
	/// <returns>The rounded value.</returns>
	private static PreciseNumber RoundToSignificantDigits(BigInteger value, int significantDigits)
	{
		int integerDigits = DigitCount(value) - DigitCount(Scale) + 1;
		int shift = significantDigits - integerDigits;

		// One digit beyond the ones being kept, to round on.
		BigInteger scaled = value * BigInteger.Pow(10, shift + 1) / Scale;
		BigInteger rounded = BigInteger.DivRem(scaled, 10, out BigInteger remainder);
		if (BigInteger.Abs(remainder) >= 5)
		{
			rounded += value.Sign;
		}

		int exponent = -shift;
		if (DigitCount(rounded) > significantDigits)
		{
			rounded /= 10;
			exponent++;
		}

		return PreciseNumber.CreateFromComponents(exponent, rounded);
	}

	/// <summary>
	/// Counts the decimal digits of a <see cref="BigInteger"/>.
	/// </summary>
	/// <param name="value">The value to count.</param>
	/// <returns>The number of decimal digits, ignoring any sign.</returns>
	private static int DigitCount(BigInteger value) =>
		BigInteger.Abs(value).ToString(CultureInfo.InvariantCulture).Length;
}
