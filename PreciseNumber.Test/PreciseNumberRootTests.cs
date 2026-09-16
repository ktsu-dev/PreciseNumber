// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber.Test;

using System.Globalization;
using System.Numerics;

/// <summary>
/// Covers <see cref="IRootFunctions{TSelf}"/> on <see cref="PreciseNumber"/>.
/// </summary>
/// <remarks>
/// The digit-for-digit assertions carry published values rather than values this library produced,
/// so a change that makes the roots agree with themselves but not with mathematics still fails. The
/// sweeps then check the property the published values cannot: that the root of an arbitrary value
/// squares back to the value it came from.
/// </remarks>
[TestClass]
public class PreciseNumberRootTests
{
	/// <summary>
	/// The first fifty significant digits of the square root of two, three and ten.
	/// </summary>
	private const string Sqrt2Digits = "14142135623730950488016887242096980785696718753769";
	private const string Sqrt3Digits = "17320508075688772935274463415058723669428052538104";
	private const string Sqrt10Digits = "31622776601683793319988935444327185337195551393252";

	/// <summary>
	/// The first fifty significant digits of the cube root of two, and of the fifth root of seven.
	/// </summary>
	private const string Cbrt2Digits = "12599210498948731647672106072782283505702514647015";
	private const string Root5Of7Digits = "14757731615945520692769166956322441065440936137402";

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

	[TestMethod]
	public void TestSqrtMatchesPublishedDigits()
	{
		Assert.AreEqual(Sqrt2Digits, Digits(PreciseNumber.Sqrt(2.ToPreciseNumber(), 50)), "Sqrt(2) is wrong");
		Assert.AreEqual(Sqrt3Digits, Digits(PreciseNumber.Sqrt(3.ToPreciseNumber(), 50)), "Sqrt(3) is wrong");
		Assert.AreEqual(Sqrt10Digits, Digits(PreciseNumber.Sqrt(10.ToPreciseNumber(), 50)), "Sqrt(10) is wrong");
	}

	[TestMethod]
	public void TestSqrtPlacesTheDecimalPoint()
	{
		Assert.AreEqual("1.4142135623730950488016887242096980785696718753769", PreciseNumber.Sqrt(2.ToPreciseNumber(), 50).ToString());
		Assert.AreEqual("3.1622776601683793319988935444327185337195551393252", PreciseNumber.Sqrt(10.ToPreciseNumber(), 50).ToString());
		Assert.AreEqual("1.414213562", PreciseNumber.Sqrt(2.ToPreciseNumber(), 10).ToString());
	}

	[TestMethod]
	public void TestSqrtRoundsTheLastDigitRatherThanTruncatingIt()
	{
		// Sqrt(3) continues ...0525381038, so the 50th digit is a 3 that rounds up to a 4. A
		// truncating implementation leaves it at 3.
		Assert.AreEqual('4', Digits(PreciseNumber.Sqrt(3.ToPreciseNumber(), 50))[^1], "Sqrt(3) truncates where it should round");
	}

	[TestMethod]
	public void TestSqrtOfAPerfectSquareIsExact()
	{
		PreciseNumber root = PreciseNumber.Sqrt(144.ToPreciseNumber());

		Assert.AreEqual(12.ToPreciseNumber(), root, "Sqrt(144) is not 12");
		Assert.AreEqual(2, root.SignificantDigits, "Sqrt(144) carries digits it does not have");
		Assert.AreEqual("12", root.ToString());
	}

	[TestMethod]
	public void TestAnExactRootIsExactWhateverPrecisionIsAskedFor()
	{
		// A perfect square whose root is far longer than the digits requested. An implementation
		// that rounds unconditionally answers 1.2e29 for the first of these.
		PreciseNumber root = Parse("123456789012345678901234567890");
		PreciseNumber square = root.Squared();

		Assert.AreEqual(root, PreciseNumber.Sqrt(square, 1), "An exact square root was rounded away");
		Assert.AreEqual(root, PreciseNumber.Sqrt(square, 50), "An exact square root was rounded away");
	}

	[TestMethod]
	public void TestSqrtHandlesValuesOutsideTheRangeOfADouble()
	{
		// 1e400 overflows a double, so an implementation that seeds from one gets these wrong.
		PreciseNumber large = PreciseNumber.Sqrt(Parse("2E400"), 50);
		PreciseNumber small = PreciseNumber.Sqrt(Parse("2E-400"), 50);

		Assert.AreEqual(Sqrt2Digits, Digits(large), "Sqrt(2e400) is wrong");
		Assert.AreEqual(151, large.Exponent, "Sqrt(2e400) has the wrong exponent");

		Assert.AreEqual(Sqrt2Digits, Digits(small), "Sqrt(2e-400) is wrong");
		Assert.AreEqual(-249, small.Exponent, "Sqrt(2e-400) has the wrong exponent");

		// An odd exponent cannot be halved by the exponent alone, so the significand carries it.
		PreciseNumber odd = PreciseNumber.Sqrt(Parse("1E401"), 50);
		Assert.AreEqual(Sqrt10Digits, Digits(odd), "Sqrt(1e401) is wrong");
		Assert.AreEqual(151, odd.Exponent, "Sqrt(1e401) has the wrong exponent");
	}

	[TestMethod]
	public void TestSqrtSquaresBackToItsInput()
	{
		string[] inputs = ["2", "3", "7", "0.5", "1E-17", "123456.789", "9.87654321E31", "1E400", "6.02214076E-23"];

		foreach (string input in inputs)
		{
			PreciseNumber value = Parse(input);
			PreciseNumber root = PreciseNumber.Sqrt(value, 50);

			AssertAgreesTo(value, root.Squared(), 48, $"Sqrt({input}) does not square back");
		}
	}

	[TestMethod]
	public void TestSqrtOfZeroAndOne()
	{
		Assert.AreEqual(PreciseNumber.Zero, PreciseNumber.Sqrt(PreciseNumber.Zero));
		Assert.AreEqual(PreciseNumber.One, PreciseNumber.Sqrt(PreciseNumber.One));
	}

	[TestMethod]
	public void TestSqrtOfANegativeValueThrows()
	{
		// There is no NaN to return, so this is a genuine divergence from double and the exception
		// type is part of the contract.
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.Sqrt((-1).ToPreciseNumber()));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.Sqrt((-1).ToPreciseNumber(), 50));
	}

	[TestMethod]
	public void TestSqrtDefaultsToTheInputsPrecisionRatherThanCappingIt()
	{
		// The rule Divide follows: never fewer digits than the operand, and never fewer than
		// MinimumDivisionPrecision. Asserted as a floor because a root whose last digit rounds to
		// zero has it stripped, as everywhere else in the library: Sqrt(Pi) is one such.
		Assert.AreEqual(PreciseNumber.MinimumDivisionPrecision, PreciseNumber.Sqrt(2.ToPreciseNumber()).SignificantDigits);

		Assert.IsGreaterThanOrEqualTo(
			PreciseNumber.ConstantPrecision - 1,
			PreciseNumber.Sqrt(PreciseNumber.Pi).SignificantDigits,
			"Sqrt(Pi) was capped at MinimumDivisionPrecision rather than following its operand");
	}

	[TestMethod]
	public void TestCbrtMatchesPublishedDigits() =>
		Assert.AreEqual(Cbrt2Digits, Digits(PreciseNumber.Cbrt(2.ToPreciseNumber(), 50)), "Cbrt(2) is wrong");

	[TestMethod]
	public void TestCbrtOfANegativeValueIsReal()
	{
		Assert.AreEqual((-2).ToPreciseNumber(), PreciseNumber.Cbrt((-8).ToPreciseNumber()), "Cbrt(-8) is not -2");
		Assert.AreEqual($"-{Cbrt2Digits}", Digits(PreciseNumber.Cbrt((-2).ToPreciseNumber(), 50)), "Cbrt(-2) has the wrong digits");
		Assert.AreEqual(PreciseNumber.Cbrt(2.ToPreciseNumber(), 50), -PreciseNumber.Cbrt((-2).ToPreciseNumber(), 50), "Cbrt is not odd about zero");
	}

	[TestMethod]
	public void TestCbrtOfAPerfectCubeIsExact()
	{
		Assert.AreEqual(7.ToPreciseNumber(), PreciseNumber.Cbrt(343.ToPreciseNumber()), "Cbrt(343) is not 7");
		Assert.AreEqual(Parse("0.2"), PreciseNumber.Cbrt(Parse("0.008")), "Cbrt(0.008) is not 0.2");
	}

	[TestMethod]
	public void TestRootNMatchesPublishedDigits() =>
		Assert.AreEqual(Root5Of7Digits, Digits(PreciseNumber.RootN(7.ToPreciseNumber(), 5, 50)), "The fifth root of 7 is wrong");

	[TestMethod]
	public void TestRootNRaisesBackToItsInput()
	{
		int[] degrees = [2, 3, 4, 5, 9, 17];

		foreach (int degree in degrees)
		{
			PreciseNumber value = Parse("1234.5678");
			PreciseNumber root = PreciseNumber.RootN(value, degree, 50);

			AssertAgreesTo(value, root.Pow(degree.ToPreciseNumber()), 45, $"The root of degree {degree} does not raise back");
		}
	}

	[TestMethod]
	public void TestRootNOfDegreeOneIsTheValueItself() =>
		Assert.AreEqual(Parse("1234.5678"), PreciseNumber.RootN(Parse("1234.5678"), 1));

	[TestMethod]
	public void TestRootNOfANegativeDegreeIsTheReciprocal()
	{
		PreciseNumber reciprocal = PreciseNumber.RootN(16.ToPreciseNumber(), -2);

		Assert.AreEqual(Parse("0.25"), reciprocal, "The -2 root of 16 is not 1/4");
		AssertAgreesTo(PreciseNumber.One / PreciseNumber.Sqrt(2.ToPreciseNumber()), PreciseNumber.RootN(2.ToPreciseNumber(), -2), 48, "The -2 root of 2 is not 1/Sqrt(2)");
	}

	[TestMethod]
	public void TestAnEvenRootOfANegativeValueThrows()
	{
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.RootN((-16).ToPreciseNumber(), 4));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.RootN((-16).ToPreciseNumber(), -4));
	}

	[TestMethod]
	public void TestAnOddRootOfANegativeValueCarriesTheSign() =>
		Assert.AreEqual((-3).ToPreciseNumber(), PreciseNumber.RootN((-243).ToPreciseNumber(), 5), "The fifth root of -243 is not -3");

	[TestMethod]
	public void TestARootOfDegreeZeroThrows()
	{
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.RootN(2.ToPreciseNumber(), 0));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.RootN(2.ToPreciseNumber(), int.MinValue));
	}

	[TestMethod]
	public void TestARootOfZeroIsZeroUnlessTheDegreeIsNegative()
	{
		Assert.AreEqual(PreciseNumber.Zero, PreciseNumber.RootN(PreciseNumber.Zero, 5));
		Assert.ThrowsExactly<DivideByZeroException>(() => PreciseNumber.RootN(PreciseNumber.Zero, -5));
	}

	[TestMethod]
	public void TestARootOfFewerThanOneDigitThrows()
	{
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.Sqrt(2.ToPreciseNumber(), 0));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.Cbrt(2.ToPreciseNumber(), -1));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.RootN(2.ToPreciseNumber(), 3, 0));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.Hypot(3.ToPreciseNumber(), 4.ToPreciseNumber(), 0));
	}

	[TestMethod]
	public void TestHypotOfAPythagoreanTripleIsExact()
	{
		Assert.AreEqual(5.ToPreciseNumber(), PreciseNumber.Hypot(3.ToPreciseNumber(), 4.ToPreciseNumber()), "Hypot(3, 4) is not 5");
		Assert.AreEqual(13.ToPreciseNumber(), PreciseNumber.Hypot(5.ToPreciseNumber(), 12.ToPreciseNumber()), "Hypot(5, 12) is not 13");
	}

	[TestMethod]
	public void TestHypotMatchesTheSquareRootOfTheSumOfSquares()
	{
		Assert.AreEqual(Sqrt2Digits, Digits(PreciseNumber.Hypot(PreciseNumber.One, PreciseNumber.One, 50)), "Hypot(1, 1) is not Sqrt(2)");
		Assert.AreEqual(5.ToPreciseNumber(), PreciseNumber.Hypot((-3).ToPreciseNumber(), (-4).ToPreciseNumber()), "Hypot does not ignore the signs");
	}

	[TestMethod]
	public void TestHypotNeedsNoScalingToAvoidOverflow()
	{
		// The magnitudes that make a double implementation overflow while squaring. There is no
		// exponent range to fall out of here, so the answer is the one the algebra gives.
		PreciseNumber large = PreciseNumber.Hypot(Parse("3E400"), Parse("4E400"));
		PreciseNumber small = PreciseNumber.Hypot(Parse("3E-400"), Parse("4E-400"));

		Assert.AreEqual(Parse("5E400"), large, "Hypot overflowed at 1e400");
		Assert.AreEqual(Parse("5E-400"), small, "Hypot underflowed at 1e-400");
	}

	[TestMethod]
	public void TestRootsAreReachableThroughTheInterface()
	{
		// The point of implementing IRootFunctions is that generic code can ask for a root without
		// knowing which number it holds.
		Assert.AreEqual(3.ToPreciseNumber(), RootThroughInterface(9.ToPreciseNumber()), "Sqrt is not reachable generically");
		Assert.AreEqual(PreciseNumber.Pi, PiThroughInterface<PreciseNumber>(), "IFloatingPointConstants is not satisfied");
	}

	private static TNumber RootThroughInterface<TNumber>(TNumber value)
		where TNumber : IRootFunctions<TNumber> =>
		TNumber.Sqrt(value);

	private static TNumber PiThroughInterface<TNumber>()
		where TNumber : IRootFunctions<TNumber> =>
		TNumber.Pi;

	[TestMethod]
	public void TestRootsOfLongSignificandsStayCorrect()
	{
		// 200 digits is the widest of the repository's benchmark digit counts, and wide enough that
		// a working precision chosen for the requested digits alone would show up here.
		BigInteger significand = BigInteger.Parse(new string('7', 200), NumberStyles.None, CultureInfo.InvariantCulture);
		PreciseNumber value = PreciseNumber.CreateFromComponents(-100, significand);

		PreciseNumber root = PreciseNumber.Sqrt(value);

		Assert.AreEqual(200, root.SignificantDigits, "A 200 digit input produced a shorter root");
		AssertAgreesTo(value, root.Squared(), 195, "The root of a 200 digit value does not square back");
	}
}
