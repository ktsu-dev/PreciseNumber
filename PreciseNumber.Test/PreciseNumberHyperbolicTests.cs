// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber.Test;

using System.Globalization;
using System.Linq;
using System.Numerics;

/// <summary>
/// Covers <see cref="IHyperbolicFunctions{TSelf}"/> on <see cref="PreciseNumber"/>.
/// </summary>
/// <remarks>
/// The digit-for-digit assertions carry values computed independently of this library, at eighty
/// digits, from the constants each function is defined against — so a change that makes the
/// implementation agree with itself but not with mathematics still fails.
/// <para>
/// Three of the small-argument assertions are load-bearing, and it is worth recording which, since
/// the set is narrower than the floating-point intuition suggests. Substituting the textbook form
/// back in was tried, one function at a time, and only <c>sinh</c>'s <c>(e^x - e^-x)/2</c>,
/// <c>asinh</c>'s <c>ln(x + √(x² + 1))</c> and <c>atanh</c>'s <c>½ ln((1 + x)/(1 - x))</c> actually
/// lose digits: each cancels against a value that <c>Exp</c>, <c>Sqrt</c> or <c>Divide</c> had
/// already rounded to the working width, and a <c>1e-30</c> argument comes back with about thirty
/// of the fifty digits asked for. <see cref="TestSinhKeepsItsDigitsNearZero"/>,
/// <see cref="TestAsinhKeepsItsDigitsNearZero"/> and <see cref="TestAtanhKeepsItsDigitsNearZero"/>
/// fail outright on those forms rather than drifting in the last place, which is what stops a later
/// simplification quietly undoing the rearrangements.
/// </para>
/// <para>
/// <see cref="TestTanhKeepsItsDigitsNearZero"/> and
/// <see cref="TestAcoshKeepsItsDigitsJustAboveOne"/> read like members of that set and are not.
/// Their textbook forms pass, because this type's addition, subtraction and multiplication are
/// exact and nothing has been rounded before the cancellation. They are kept as ordinary regression
/// assertions — the values they pin are still the right ones — but they should not be relied on to
/// catch a rewrite of those two functions.
/// </para>
/// </remarks>
[TestClass]
public class PreciseNumberHyperbolicTests
{
	/// <summary>
	/// The first fifty significant digits of the hyperbolic functions of one.
	/// </summary>
	private const string Sinh1Digits = "11752011936438014568823818505956008151557179813341";
	private const string Cosh1Digits = "15430806348152437784779056207570616826015291123659";
	private const string Tanh1Digits = "76159415595576488811945828260479359041276859725794";

	/// <summary>
	/// The first fifty significant digits of the inverse hyperbolic functions, each of which is a
	/// logarithm in closed form: <c>asinh 1 = ln(1 + √2)</c>, <c>acosh 2 = ln(2 + √3)</c> and
	/// <c>atanh ½ = ½ ln 3</c>.
	/// </summary>
	private const string Asinh1Digits = "88137358701954302523260932497979230902816032826164";
	private const string Acosh2Digits = "13169578969248167086250463473079684440269819714675";
	private const string AtanhHalfDigits = "54930614433405484569762261846126285232374527891137";

	/// <summary>
	/// The first fifty significant digits of <c>acosh(1 + 1e-30)</c>, which is <c>1.414…e-15</c>.
	/// </summary>
	/// <remarks>
	/// Close to <c>√(2 · 1e-30)</c> but not equal to it: <c>acosh(1 + d) = √(2d) · (1 - d/12 + …)</c>,
	/// so this and <c>√2</c> share their first twenty-nine digits and then diverge. Comparing against
	/// the true value rather than against <c>√2</c> is what makes the assertion mean something.
	/// </remarks>
	private const string AcoshJustAboveOneDigits = "14142135623730950488016887242095802274394741174562";

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

		Assert.IsLessThanOrEqualTo(
			tolerance,
			difference,
			$"{message}: expected {expected}, got {actual}, which differs by {difference}");
	}

	/// <summary>
	/// A spread that straddles the magnitude at which <c>Sinh</c> stops summing its own series and
	/// defers to <c>Exp</c>, and includes both signs of each.
	/// </summary>
	private static PreciseNumber[] Sweep() =>
	[
		Parse("1E-30"),
		Parse("-1E-30"),
		Parse("0.25"),
		Parse("-0.25"),
		Parse("0.5"),
		Parse("1"),
		Parse("-1"),
		Parse("2.75"),
		Parse("-2.75"),
	];

	[TestMethod]
	public void TestSinhMatchesPublishedDigits() =>
		Assert.AreEqual(Sinh1Digits, Digits(PreciseNumber.Sinh(PreciseNumber.One, 50)), "Sinh(1) is wrong");

	[TestMethod]
	public void TestCoshMatchesPublishedDigits() =>
		Assert.AreEqual(Cosh1Digits, Digits(PreciseNumber.Cosh(PreciseNumber.One, 50)), "Cosh(1) is wrong");

	[TestMethod]
	public void TestTanhMatchesPublishedDigits() =>
		Assert.AreEqual(Tanh1Digits, Digits(PreciseNumber.Tanh(PreciseNumber.One, 50)), "Tanh(1) is wrong");

	[TestMethod]
	public void TestAsinhMatchesPublishedDigits() =>
		Assert.AreEqual(Asinh1Digits, Digits(PreciseNumber.Asinh(PreciseNumber.One, 50)), "Asinh(1) is wrong");

	[TestMethod]
	public void TestAcoshMatchesPublishedDigits() =>
		Assert.AreEqual(Acosh2Digits, Digits(PreciseNumber.Acosh(2.ToPreciseNumber(), 50)), "Acosh(2) is wrong");

	[TestMethod]
	public void TestAtanhMatchesPublishedDigits() =>
		Assert.AreEqual(AtanhHalfDigits, Digits(PreciseNumber.Atanh(Parse("0.5"), 50)), "Atanh(0.5) is wrong");

	/// <summary>
	/// Pins the series path against the identity that defines it.
	/// </summary>
	/// <remarks>
	/// <c>sinh</c> below one half is summed rather than taken from two exponentials, so this is the
	/// assertion that the two paths agree on the answer and not merely on the neighbourhood.
	/// </remarks>
	[TestMethod]
	public void TestCoshSquaredLessSinhSquaredIsOneAcrossASweep()
	{
		foreach (PreciseNumber x in Sweep())
		{
			PreciseNumber sinh = PreciseNumber.Sinh(x, 50);
			PreciseNumber cosh = PreciseNumber.Cosh(x, 50);
			AssertAgreesTo(
				PreciseNumber.One,
				(cosh * cosh) - (sinh * sinh),
				45,
				$"cosh²x - sinh²x is not one at x = {x}");
		}
	}

	[TestMethod]
	public void TestTanhIsSinhOverCoshAcrossASweep()
	{
		foreach (PreciseNumber x in Sweep())
		{
			PreciseNumber expected = PreciseNumber.Divide(
				PreciseNumber.Sinh(x, 60),
				PreciseNumber.Cosh(x, 60),
				50);

			AssertAgreesTo(expected, PreciseNumber.Tanh(x, 50), 45, $"Tanh disagrees with Sinh/Cosh at x = {x}");
		}
	}

	[TestMethod]
	public void TestSinhAndAsinhRoundTripAcrossASweep()
	{
		foreach (PreciseNumber x in Sweep())
		{
			AssertAgreesTo(x, PreciseNumber.Asinh(PreciseNumber.Sinh(x, 60), 50), 45, $"Asinh(Sinh(x)) lost x = {x}");
		}
	}

	[TestMethod]
	public void TestTanhAndAtanhRoundTripAcrossASweep()
	{
		foreach (PreciseNumber x in Sweep())
		{
			AssertAgreesTo(x, PreciseNumber.Atanh(PreciseNumber.Tanh(x, 60), 50), 45, $"Atanh(Tanh(x)) lost x = {x}");
		}
	}

	/// <summary>
	/// <c>acosh</c> is the inverse of <c>cosh</c> only on the non-negative half, since <c>cosh</c> is
	/// even, so the round trip returns the magnitude rather than the value.
	/// </summary>
	/// <remarks>
	/// The sweep's smallest values are left out, and the reason is the function rather than the
	/// implementation: <c>cosh</c> is flat at zero, so <c>cosh(1e-30) - 1</c> is <c>5e-61</c> and
	/// falls below any working precision short of sixty-one digits. Once <c>cosh</c> has rounded to
	/// exactly one there is no <c>acosh</c> that can recover the argument, and asserting otherwise
	/// would be asserting against arithmetic rather than against this code.
	/// </remarks>
	[TestMethod]
	public void TestCoshAndAcoshRoundTripToTheMagnitudeAcrossASweep()
	{
		foreach (PreciseNumber x in Sweep().Where(x => PreciseNumber.Abs(x) >= Parse("0.25")))
		{
			PreciseNumber recovered = PreciseNumber.Acosh(PreciseNumber.Cosh(x, 60), 50);
			AssertAgreesTo(PreciseNumber.Abs(x), recovered, 40, $"Acosh(Cosh(x)) lost |x| at x = {x}");
		}
	}

	/// <summary>
	/// The assertion that fails on <c>(e^x - e^-x) / 2</c>.
	/// </summary>
	/// <remarks>
	/// Both exponentials are one to thirty digits at this argument, so the textbook difference keeps
	/// only the digits that survive it — about thirty of the fifty asked for. The series keeps all
	/// fifty, and forty-five is comfortably on the far side of the gap between them.
	/// </remarks>
	[TestMethod]
	public void TestSinhKeepsItsDigitsNearZero()
	{
		PreciseNumber tiny = Parse("1E-30");
		AssertAgreesTo(tiny, PreciseNumber.Sinh(tiny, 50), 45, "Sinh(1e-30) has lost its precision");
		Assert.AreNotEqual(PreciseNumber.Zero, PreciseNumber.Sinh(tiny, 50), "Sinh(1e-30) collapsed to zero");
	}

	/// <summary>
	/// The assertion that fails on <c>ln(x + √(x² + 1))</c>.
	/// </summary>
	[TestMethod]
	public void TestAsinhKeepsItsDigitsNearZero()
	{
		PreciseNumber tiny = Parse("1E-30");
		AssertAgreesTo(tiny, PreciseNumber.Asinh(tiny, 50), 45, "Asinh(1e-30) has lost its precision");
	}

	/// <summary>
	/// The assertion that fails on <c>½ ln((1 + x)/(1 - x))</c>.
	/// </summary>
	[TestMethod]
	public void TestAtanhKeepsItsDigitsNearZero()
	{
		PreciseNumber tiny = Parse("1E-30");
		AssertAgreesTo(tiny, PreciseNumber.Atanh(tiny, 50), 45, "Atanh(1e-30) has lost its precision");
	}

	/// <summary>
	/// Pins <c>tanh</c> near zero. Not a discriminating assertion — see the note on this class.
	/// </summary>
	/// <remarks>
	/// A <c>tanh</c> built as <c>(1 - e^-2x) / (1 + e^-2x)</c> against a literal one passes this too,
	/// because the subtraction is exact and the exponential has not been rounded against anything
	/// first. The value is still worth pinning; it just does not police the implementation the way
	/// <see cref="TestSinhKeepsItsDigitsNearZero"/> does.
	/// </remarks>
	[TestMethod]
	public void TestTanhKeepsItsDigitsNearZero()
	{
		PreciseNumber tiny = Parse("1E-30");
		AssertAgreesTo(tiny, PreciseNumber.Tanh(tiny, 50), 45, "Tanh(1e-30) has lost its precision");
	}

	/// <summary>
	/// Pins <c>acosh</c> just above one, where the answer is most easily got wrong.
	/// </summary>
	/// <remarks>
	/// The unfactored <c>ln(x + √(x² - 1))</c> passes this as well — squaring and subtracting are
	/// both exact here, so the cancellation costs nothing but width. What the assertion does catch is
	/// an answer that is merely plausible: the expected value is <em>not</em> <c>√(2e-30)</c>, since
	/// <c>acosh(1 + d) = √(2d) · (1 - d/12 + …)</c>, so the two share their first twenty-nine digits
	/// and then part. A rearrangement that kept the precision but dropped the correction term would
	/// look right to any shorter comparison and fails this one.
	/// </remarks>
	[TestMethod]
	public void TestAcoshKeepsItsDigitsJustAboveOne()
	{
		PreciseNumber justAbove = Parse("1.000000000000000000000000000001");
		Assert.AreEqual(
			AcoshJustAboveOneDigits,
			Digits(PreciseNumber.Acosh(justAbove, 50)),
			"Acosh just above one is wrong");
	}

	[TestMethod]
	public void TestHyperbolicFunctionsOfZeroAreExact()
	{
		Assert.AreEqual(PreciseNumber.Zero, PreciseNumber.Sinh(PreciseNumber.Zero));
		Assert.AreEqual(PreciseNumber.One, PreciseNumber.Cosh(PreciseNumber.Zero));
		Assert.AreEqual(PreciseNumber.Zero, PreciseNumber.Tanh(PreciseNumber.Zero));
		Assert.AreEqual(PreciseNumber.Zero, PreciseNumber.Asinh(PreciseNumber.Zero));
		Assert.AreEqual(PreciseNumber.Zero, PreciseNumber.Acosh(PreciseNumber.One));
		Assert.AreEqual(PreciseNumber.Zero, PreciseNumber.Atanh(PreciseNumber.Zero));
	}

	[TestMethod]
	public void TestSinhTanhAsinhAndAtanhAreOddAndCoshIsEven()
	{
		foreach (PreciseNumber x in Sweep())
		{
			Assert.AreEqual(-PreciseNumber.Sinh(x, 50), PreciseNumber.Sinh(-x, 50), $"Sinh is not odd at x = {x}");
			Assert.AreEqual(-PreciseNumber.Tanh(x, 50), PreciseNumber.Tanh(-x, 50), $"Tanh is not odd at x = {x}");
			Assert.AreEqual(-PreciseNumber.Asinh(x, 50), PreciseNumber.Asinh(-x, 50), $"Asinh is not odd at x = {x}");
			Assert.AreEqual(PreciseNumber.Cosh(x, 50), PreciseNumber.Cosh(-x, 50), $"Cosh is not even at x = {x}");
		}
	}

	/// <summary>
	/// Pins the saturation branch, and with it the claim that <c>Tanh</c> cannot overflow.
	/// </summary>
	/// <remarks>
	/// <c>Tanh</c> takes the exponential of minus twice its argument, so a large argument sends that
	/// exponential towards zero rather than towards an unrepresentable magnitude. Past the point
	/// where it falls below the last digit being carried the answer is <c>±1</c> exactly, and is
	/// returned without the exponential being taken at all — which is what keeps <c>1e15</c> below
	/// from throwing, since <c>e^-2e15</c> needs an exponent far outside an <see cref="int"/>.
	/// </remarks>
	[TestMethod]
	public void TestTanhSaturatesRatherThanOverflowing()
	{
		Assert.AreEqual(PreciseNumber.One, PreciseNumber.Tanh(1000.ToPreciseNumber(), 50));
		Assert.AreEqual(PreciseNumber.NegativeOne, PreciseNumber.Tanh((-1000).ToPreciseNumber(), 50));
		Assert.AreEqual(PreciseNumber.One, PreciseNumber.Tanh(Parse("1E15"), 50));
		Assert.AreEqual(PreciseNumber.NegativeOne, PreciseNumber.Tanh(Parse("-1E15"), 50));
	}

	/// <summary>
	/// Just below saturation the answer is still strictly inside <c>(-1, 1)</c>, so the threshold is
	/// pinned from both sides rather than only from the far one.
	/// </summary>
	[TestMethod]
	public void TestTanhBelowSaturationIsStrictlyInsideItsRange()
	{
		PreciseNumber tanh = PreciseNumber.Tanh(20.ToPreciseNumber(), 50);

		Assert.IsLessThan(PreciseNumber.One, tanh, "Tanh(20) reached one");
		AssertAgreesTo(PreciseNumber.One, tanh, 16, "Tanh(20) is not close to one");
	}

	[TestMethod]
	public void TestHyperbolicFunctionsRejectValuesOutsideTheirDomain()
	{
		// There is no NaN and no infinity to return, so the domain is enforced rather than encoded.
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.Acosh(Parse("0.5")));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.Acosh(PreciseNumber.Zero));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.Acosh(PreciseNumber.NegativeOne));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.Atanh(PreciseNumber.One));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.Atanh(PreciseNumber.NegativeOne));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.Atanh(2.ToPreciseNumber()));
	}

	[TestMethod]
	public void TestHyperbolicFunctionsRejectAPrecisionBelowOne()
	{
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.Sinh(PreciseNumber.One, 0));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.Cosh(PreciseNumber.One, 0));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.Tanh(PreciseNumber.One, 0));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.Asinh(PreciseNumber.One, 0));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.Acosh(2.ToPreciseNumber(), 0));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.Atanh(PreciseNumber.Zero, 0));
	}

	/// <summary>
	/// The cheap regression net: agreement with the runtime's own implementations to fifteen digits,
	/// which is about all a <see cref="double"/> carries.
	/// </summary>
	[TestMethod]
	public void TestHyperbolicFunctionsAgreeWithTheRuntimeToFifteenDigits()
	{
		foreach (PreciseNumber x in Sweep())
		{
			double value = x.To<double>();

			AssertAgreesTo(Math.Sinh(value).ToPreciseNumber(), PreciseNumber.Sinh(x, 50), 14, $"Sinh disagrees at x = {x}");
			AssertAgreesTo(Math.Cosh(value).ToPreciseNumber(), PreciseNumber.Cosh(x, 50), 14, $"Cosh disagrees at x = {x}");
			AssertAgreesTo(Math.Tanh(value).ToPreciseNumber(), PreciseNumber.Tanh(x, 50), 14, $"Tanh disagrees at x = {x}");
			AssertAgreesTo(Math.Asinh(value).ToPreciseNumber(), PreciseNumber.Asinh(x, 50), 14, $"Asinh disagrees at x = {x}");
		}
	}

	/// <summary>
	/// The inverses have narrower domains than the sweep, so they get their own comparison against
	/// the runtime over values each of them accepts.
	/// </summary>
	[TestMethod]
	public void TestInverseHyperbolicFunctionsAgreeWithTheRuntimeToFifteenDigits()
	{
		foreach (PreciseNumber x in new[] { Parse("0.125"), Parse("-0.125"), Parse("0.75"), Parse("-0.75"), Parse("0.9") })
		{
			double value = x.To<double>();
			AssertAgreesTo(Math.Atanh(value).ToPreciseNumber(), PreciseNumber.Atanh(x, 50), 14, $"Atanh disagrees at x = {x}");
		}

		foreach (PreciseNumber x in new[] { Parse("1.5"), Parse("2"), Parse("10"), Parse("1000") })
		{
			double value = x.To<double>();
			AssertAgreesTo(Math.Acosh(value).ToPreciseNumber(), PreciseNumber.Acosh(x, 50), 14, $"Acosh disagrees at x = {x}");
		}
	}

	/// <summary>
	/// A large argument still produces a result, rather than reaching the exponent limit on the way.
	/// </summary>
	/// <remarks>
	/// The identity is checked at a hundred and forty digits rather than at fifty, and the reason is
	/// worth stating because the obvious version of this test is wrong. <c>sinh</c> and <c>cosh</c>
	/// of a hundred are both about <c>1.34e43</c> and differ by <c>e^-100</c>, so their squares are
	/// about <c>1.8e86</c> and a difference of one first becomes visible at the eighty-seventh
	/// significant digit. At fifty digits the two are the same number and <c>cosh² - sinh²</c> is
	/// exactly zero — correctly, not defectively. A hundred and forty leaves the identity fifty-odd
	/// digits of room to hold in.
	/// </remarks>
	[TestMethod]
	public void TestSinhAndCoshOfALargeArgument()
	{
		PreciseNumber hundred = 100.ToPreciseNumber();
		PreciseNumber sinh = PreciseNumber.Sinh(hundred, 140);
		PreciseNumber cosh = PreciseNumber.Cosh(hundred, 140);

		AssertAgreesTo(PreciseNumber.One, (cosh * cosh) - (sinh * sinh), 40, "cosh²x - sinh²x is not one at x = 100");
		AssertAgreesTo(Math.Sinh(100.0).ToPreciseNumber(), sinh, 14, "Sinh(100) disagrees with the runtime");
	}

	/// <summary>
	/// The default overload produces at least <see cref="PreciseNumber.MinimumDivisionPrecision"/>
	/// digits, and a wider argument carries its own width through.
	/// </summary>
	[TestMethod]
	public void TestDefaultPrecisionFollowsTheArgument()
	{
		Assert.AreEqual(50, PreciseNumber.Sinh(PreciseNumber.One).SignificantDigits);
		Assert.AreEqual(50, PreciseNumber.Cosh(PreciseNumber.One).SignificantDigits);

		PreciseNumber wide = Parse("1.00000000000000000000000000000000000000000000000000000000001");
		Assert.IsGreaterThanOrEqualTo(
			wide.SignificantDigits,
			PreciseNumber.Sinh(wide).SignificantDigits,
			"Sinh narrowed a wider argument");
	}
}
