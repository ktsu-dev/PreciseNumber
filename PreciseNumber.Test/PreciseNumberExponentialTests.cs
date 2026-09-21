// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber.Test;

using System.Globalization;
using System.Numerics;

/// <summary>
/// Covers <see cref="IExponentialFunctions{TSelf}"/>, <see cref="ILogarithmicFunctions{TSelf}"/> and
/// <see cref="IPowerFunctions{TSelf}"/> on <see cref="PreciseNumber"/>.
/// </summary>
/// <remarks>
/// The digit-for-digit assertions carry published values rather than values this library produced,
/// so a change that makes the series agree with themselves but not with mathematics still fails.
/// <para>
/// Several of these assertions exist specifically to pin the type's precision claim. Before these
/// functions were implemented on the significand, a fractional power went out to
/// <see cref="Math.Log(double)"/> and <see cref="Math.Exp(double)"/> and came back with about
/// fifteen correct digits wearing a fifty-digit type. Any assertion here that compares fifty
/// published digits against a fractional power or an exponential fails outright on that
/// implementation — it does not merely drift in the last place.
/// </para>
/// </remarks>
[TestClass]
public class PreciseNumberExponentialTests
{
	/// <summary>
	/// The first fifty significant digits of the natural logarithm of two, ten and three halves.
	/// </summary>
	private const string Ln2Digits = "69314718055994530941723212145817656807550013436026";
	private const string Ln10Digits = "23025850929940456840179914546843642076011014886288";
	private const string Ln1Point5Digits = "40546510810816438197801311546434913657199042346249";

	/// <summary>
	/// The first fifty significant digits of e squared, and of e to the power of minus 3.7.
	/// </summary>
	private const string ESquaredDigits = "73890560989306502272304274605750078131803155705518";
	private const string ExpNegative3Point7Digits = "24723526470339391202757382983402629344505070337871";

	/// <summary>
	/// The first fifty significant digits of two to the half, ten to the third, and two to the 3.5.
	/// </summary>
	private const string Root2Digits = "14142135623730950488016887242096980785696718753769";
	private const string CubeRoot10Digits = "21544346900318837217592935665193504952593449421921";
	private const string TwoToThreeAndAHalfDigits = "11313708498984760390413509793677584628557375003016";

	/// <summary>
	/// The first fifty significant digits of the base-10 logarithm of two, and the base-2 logarithm of ten.
	/// </summary>
	private const string Log10Of2Digits = "30102999566398119521373889472449302676818988146211";
	private const string Log2Of10Digits = "33219280948873623478703194294893901758648313930246";

	private static PreciseNumber Parse(string text) =>
		PreciseNumber.Parse(text, CultureInfo.InvariantCulture);

	private static string Digits(PreciseNumber value) =>
		value.Significand.ToString(CultureInfo.InvariantCulture);

	/// <summary>
	/// Asserts that two values agree to a number of significant digits, comparing relative to the
	/// expected magnitude so the assertion means the same thing at every exponent.
	/// </summary>
	private static void AssertAgreesTo(PreciseNumber expected, PreciseNumber actual, int digits, string message)
	{
		PreciseNumber difference = PreciseNumber.Abs(actual - expected);
		PreciseNumber tolerance = PreciseNumber.Abs(expected) * Parse($"1E-{digits.ToString(CultureInfo.InvariantCulture)}");

		Assert.IsTrue(
			difference <= tolerance,
			$"{message}: expected {expected}, got {actual}, which differs by {difference}");
	}

	/// <summary>
	/// A spread of magnitudes and digit counts, including values whose significands are far wider
	/// than a <see cref="double"/> can hold.
	/// </summary>
	private static PreciseNumber[] Sweep() =>
	[
		Parse("1.0000000000000000000000000000000000000000000000001"),
		Parse("1.5"),
		Parse("2"),
		Parse("3.1622776601683793319988935444327185337195551393252"),
		Parse("7"),
		Parse("9.9999999999999999999999999999999999999999999999999"),
		Parse("123.456789012345678901234567890123456789012345"),
		Parse("0.000000000000000000000000000000000000000000001234567"),
		Parse("6.02214076E23"),
		Parse("1E-300"),
		Parse("1E300"),
	];

	[TestMethod]
	public void TestLogMatchesPublishedDigits()
	{
		Assert.AreEqual(Ln2Digits, Digits(PreciseNumber.Log(2.ToPreciseNumber(), 50)), "Log(2) is wrong");
		Assert.AreEqual(Ln10Digits, Digits(PreciseNumber.Log(10.ToPreciseNumber(), 50)), "Log(10) is wrong");
		Assert.AreEqual(Ln1Point5Digits, Digits(PreciseNumber.Log(Parse("1.5"), 50)), "Log(1.5) is wrong");
	}

	[TestMethod]
	public void TestLogAgreesWithTheStoredConstants()
	{
		// The constants were sourced independently of the series, so agreeing with them at fifty
		// digits is a check on the series rather than on itself.
		Assert.AreEqual(PreciseNumber.Ln2To(50), PreciseNumber.Log(2.ToPreciseNumber(), 50), "Log(2) disagrees with Ln2");
		Assert.AreEqual(PreciseNumber.Ln10To(50), PreciseNumber.Log(10.ToPreciseNumber(), 50), "Log(10) disagrees with Ln10");
	}

	[TestMethod]
	public void TestLogPlacesTheDecimalPoint()
	{
		Assert.AreEqual("0.69314718055994530941723212145817656807550013436026", PreciseNumber.Log(2.ToPreciseNumber(), 50).ToString());
		Assert.AreEqual("0.6931471806", PreciseNumber.Log(2.ToPreciseNumber(), 10).ToString());
	}

	[TestMethod]
	public void TestLogOfOneIsExactlyZero() =>
		Assert.AreEqual(PreciseNumber.Zero, PreciseNumber.Log(PreciseNumber.One));

	[TestMethod]
	public void TestLogRejectsValuesOutsideItsDomain()
	{
		// There is no NaN and no infinity to return, so the domain is enforced rather than encoded.
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.Log(PreciseNumber.Zero));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.Log(PreciseNumber.NegativeOne));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.Log(2.ToPreciseNumber(), 0));
	}

	[TestMethod]
	public void TestExpMatchesPublishedDigits()
	{
		Assert.AreEqual(ESquaredDigits, Digits(PreciseNumber.Exp(2.ToPreciseNumber(), 50)), "Exp(2) is wrong");
		Assert.AreEqual(ExpNegative3Point7Digits, Digits(PreciseNumber.Exp(Parse("-3.7"), 50)), "Exp(-3.7) is wrong");
	}

	[TestMethod]
	public void TestExpOfZeroAndOne()
	{
		Assert.AreEqual(PreciseNumber.One, PreciseNumber.Exp(PreciseNumber.Zero));
		Assert.AreEqual(PreciseNumber.ETo(50), PreciseNumber.Exp(PreciseNumber.One, 50));
	}

	[TestMethod]
	public void TestExpAndLogRoundTripAcrossASweep()
	{
		// The logarithm is carried wider than the answer is wanted, because Exp inverts an absolute
		// error into a relative one: ln(1E-300) has three integer digits, so a fifty-digit logarithm
		// only pins forty-seven digits of the value it came from.
		foreach (PreciseNumber value in Sweep())
		{
			PreciseNumber roundTripped = PreciseNumber.Exp(PreciseNumber.Log(value, 60), 60);
			AssertAgreesTo(value, roundTripped, 49, $"Exp(Log({value})) did not return its argument");
		}
	}

	[TestMethod]
	public void TestLogAndExpRoundTripAcrossASweep()
	{
		foreach (PreciseNumber value in Sweep())
		{
			// Log of an exponential, rather than the other way round, so the range reduction inside
			// Exp is the thing being inverted.
			PreciseNumber exponent = PreciseNumber.Log(value, 50);
			AssertAgreesTo(exponent, PreciseNumber.Log(PreciseNumber.Exp(exponent, 50), 50), 48, $"Log(Exp({exponent})) did not return its argument");
		}
	}

	[TestMethod]
	public void TestPowWithAnIntegerExponentStaysExact()
	{
		// The integer path is exact today and must not be traded away for the fractional one. An
		// exact answer has no tolerance to compare against, so this is equality, not agreement.
		foreach (PreciseNumber value in Sweep())
		{
			Assert.AreEqual(value.Squared(), value.Pow(2.ToPreciseNumber()), $"Pow({value}, 2) is not exactly its square");
			Assert.AreEqual(value.Cubed(), value.Pow(3.ToPreciseNumber()), $"Pow({value}, 3) is not exactly its cube");
		}

		Assert.AreEqual(Parse("1024"), 2.ToPreciseNumber().Pow(10.ToPreciseNumber()), "Pow(2, 10) is not exactly 1024");
	}

	[TestMethod]
	public void TestPowWithAFractionalExponentMatchesPublishedDigits()
	{
		Assert.AreEqual(Root2Digits, Digits(2.ToPreciseNumber().Pow(Parse("0.5"))), "Pow(2, 0.5) is wrong");
		Assert.AreEqual(CubeRoot10Digits, Digits(PreciseNumber.Pow(10.ToPreciseNumber(), PreciseNumber.Divide(PreciseNumber.One, 3.ToPreciseNumber(), 55)).ReduceSignificance(50)), "Pow(10, 1/3) is wrong");
		Assert.AreEqual(TwoToThreeAndAHalfDigits, Digits(2.ToPreciseNumber().Pow(Parse("3.5"))), "Pow(2, 3.5) is wrong");
	}

	[TestMethod]
	public void TestPowWithAFractionalExponentAgreesWithTheRoots()
	{
		// Two independent routes to the same answer: the integer Newton root on the significand, and
		// exp(y · ln x). Fifteen of these digits would agree under a double fallback; forty-eight
		// only agree if neither route went near one.
		foreach (PreciseNumber value in Sweep())
		{
			AssertAgreesTo(PreciseNumber.Sqrt(value, 50), value.Pow(Parse("0.5")), 48, $"Pow({value}, 0.5) disagrees with Sqrt");
		}
	}

	[TestMethod]
	public void TestPowRejectsAFractionalPowerOfANegativeValue()
	{
		// An odd integer power of a negative value is real and still allowed.
		Assert.AreEqual(Parse("-8"), Parse("-2").Pow(3.ToPreciseNumber()));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Parse("-2").Pow(Parse("0.5")));
	}

	[TestMethod]
	public void TestExpM1KeepsTheDigitsOfASmallArgument()
	{
		// Computed as Exp(x) - 1 this is exactly zero: every digit of the answer lies below the last
		// digit the subtraction kept.
		PreciseNumber result = PreciseNumber.ExpM1(Parse("1E-30"), 50);

		Assert.AreNotEqual(PreciseNumber.Zero, result, "ExpM1 of a small argument collapsed to zero");
		AssertAgreesTo(Parse("1.0000000000000000000000000000005E-30"), result, 40, "ExpM1(1E-30) is wrong");
	}

	[TestMethod]
	public void TestLogP1KeepsTheDigitsOfASmallArgument()
	{
		PreciseNumber result = PreciseNumber.LogP1(Parse("1E-30"), 50);

		Assert.AreNotEqual(PreciseNumber.Zero, result, "LogP1 of a small argument collapsed to zero");
		AssertAgreesTo(Parse("9.999999999999999999999999999995E-31"), result, 40, "LogP1(1E-30) is wrong");
	}

	[TestMethod]
	public void TestExpM1AndLogP1InvertEachOther()
	{
		foreach (string text in new[] { "1E-30", "-1E-30", "0.25", "-0.25", "3", "-3" })
		{
			PreciseNumber value = Parse(text);
			AssertAgreesTo(value, PreciseNumber.LogP1(PreciseNumber.ExpM1(value, 55), 55), 45, $"LogP1(ExpM1({value})) did not return its argument");
		}
	}

	[TestMethod]
	public void TestExp10OfAnIntegerIsAnExponentAndNothingElse()
	{
		PreciseNumber result = PreciseNumber.Exp10(50.ToPreciseNumber());

		Assert.AreEqual(BigInteger.One, result.Significand, "Exp10 of an integer ran a series instead of shifting the exponent");
		Assert.AreEqual(50, result.Exponent);
		Assert.AreEqual(1, result.SignificantDigits);
		Assert.AreEqual(Parse("1E-30"), PreciseNumber.Exp10(Parse("-30")));
	}

	[TestMethod]
	public void TestExp10AndLog10InvertEachOther()
	{
		AssertAgreesTo(Parse("2"), PreciseNumber.Exp10(PreciseNumber.Log10(2.ToPreciseNumber(), 50), 50), 48, "Exp10(Log10(2)) did not return two");
		Assert.AreEqual(Log10Of2Digits, Digits(PreciseNumber.Log10(2.ToPreciseNumber(), 50)), "Log10(2) is wrong");
	}

	[TestMethod]
	public void TestLog10OfAPowerOfTenIsExact()
	{
		// The exponent is the whole answer, so no series runs and nothing is rounded.
		Assert.AreEqual(50.ToPreciseNumber(), PreciseNumber.Log10(Parse("1E50")));
		Assert.AreEqual(Parse("-30"), PreciseNumber.Log10(Parse("1E-30")));
		Assert.AreEqual(PreciseNumber.Zero, PreciseNumber.Log10(PreciseNumber.One));
		Assert.AreEqual(3.ToPreciseNumber(), PreciseNumber.Log10(Parse("1000")));
	}

	[TestMethod]
	public void TestLog2MatchesPublishedDigits()
	{
		Assert.AreEqual(Log2Of10Digits, Digits(PreciseNumber.Log2(10.ToPreciseNumber(), 50)), "Log2(10) is wrong");
		AssertAgreesTo(10.ToPreciseNumber(), PreciseNumber.Log2(Parse("1024"), 50), 48, "Log2(1024) is wrong");
	}

	[TestMethod]
	public void TestLogInAChosenBase()
	{
		AssertAgreesTo(3.ToPreciseNumber(), PreciseNumber.Log(Parse("1000"), 10.ToPreciseNumber(), 50), 48, "Log(1000, 10) is wrong");
		AssertAgreesTo(Parse("0.5"), PreciseNumber.Log(3.ToPreciseNumber(), 9.ToPreciseNumber(), 50), 48, "Log(3, 9) is wrong");
	}

	[TestMethod]
	public void TestExp2MatchesPublishedDigits()
	{
		Assert.AreEqual(TwoToThreeAndAHalfDigits, Digits(PreciseNumber.Exp2(Parse("3.5"), 50)), "Exp2(3.5) is wrong");

		// An integer exponent goes through repeated squaring, which is exact.
		Assert.AreEqual(Parse("1024"), PreciseNumber.Exp2(10.ToPreciseNumber()));
		Assert.AreEqual(PreciseNumber.One, PreciseNumber.Exp2(PreciseNumber.Zero));
	}

	[TestMethod]
	public void TestExp2M1AndExp10M1KeepTheDigitsOfASmallArgument()
	{
		// Published values, not the first-order approximations x·ln2 and x·ln10 — those only agree
		// to thirty digits, which would let a second-order error through unnoticed.
		AssertAgreesTo(
			Parse("6.931471805599453094172321214584167945824592350726E-31"),
			PreciseNumber.Exp2M1(Parse("1E-30"), 50),
			48,
			"Exp2M1(1E-30) is wrong");

		AssertAgreesTo(
			Parse("2.3025850929940456840179914546870151566563406876341E-30"),
			PreciseNumber.Exp10M1(Parse("1E-30"), 50),
			48,
			"Exp10M1(1E-30) is wrong");
	}

	[TestMethod]
	public void TestLog2P1AndLog10P1KeepTheDigitsOfASmallArgument()
	{
		AssertAgreesTo(
			Parse("1.4426950408889634073599246810011707899062014724493E-30"),
			PreciseNumber.Log2P1(Parse("1E-30"), 50),
			48,
			"Log2P1(1E-30) is wrong");

		AssertAgreesTo(
			Parse("4.3429448190325182765112891891638793505344537988984E-31"),
			PreciseNumber.Log10P1(Parse("1E-30"), 50),
			48,
			"Log10P1(1E-30) is wrong");
	}

	[TestMethod]
	public void TestPrecisionFollowsTheWiderOperandRatherThanDouble()
	{
		// A fifty-digit answer is the point. A double round trip caps at about seventeen, so this
		// counts digits rather than comparing them.
		PreciseNumber result = PreciseNumber.Log(Parse("1.234567890123456789012345678901234567890123456789"), 50);

		Assert.AreEqual(50, result.SignificantDigits, "Log did not produce the digits it was asked for");
		Assert.AreEqual(50, PreciseNumber.Exp(Parse("1.5"), 50).SignificantDigits, "Exp did not produce the digits it was asked for");
	}

	[TestMethod]
	public void TestExponentialRejectsAnArgumentItCannotRepresent() =>
		Assert.ThrowsExactly<OverflowException>(() => PreciseNumber.Exp(Parse("1E30")));
}
