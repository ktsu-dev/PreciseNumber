// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber.Test;

using System.Globalization;
using System.Numerics;

/// <summary>
/// Covers <see cref="ITrigonometricFunctions{TSelf}"/> on <see cref="PreciseNumber"/>, together with
/// the bespoke <see cref="PreciseNumber.Atan2(PreciseNumber, PreciseNumber)"/>.
/// </summary>
/// <remarks>
/// The digit-for-digit assertions carry published values rather than values this library produced,
/// so a series that agrees with itself but not with mathematics still fails.
/// <para>
/// Several assertions here exist to pin the type's precision claim against the constant it reduces
/// by. <see cref="TestSinReducesALargeArgumentAgainstAWidePi"/> in particular carries a reference
/// far wider than a <see cref="double"/> can hold: reducing an argument of magnitude <c>10^6</c> to
/// that many digits is only possible against a π of at least that width, so the same test that
/// confirms the answer also confirms the reduction reads π wide rather than capped.
/// </para>
/// </remarks>
[TestClass]
public class PreciseNumberTrigonometryTests
{
	/// <summary>The first fifty significant digits of the sine, cosine and tangent of one radian.</summary>
	private const string Sin1Digits = "84147098480789650665250232163029899962256306079837";
	private const string Cos1Digits = "54030230586813971740093660744297660373231042061792";
	private const string Tan1Digits = "15574077246549022305069748074583601730872507723815";

	/// <summary>The first fifty significant digits of <c>π/6</c> and <c>π/4</c>.</summary>
	private const string PiOverSixDigits = "52359877559829887307710723054658381403286156656252";
	private const string PiOverFourDigits = "78539816339744830961566084581987572104929234984378";

	/// <summary><c>π/6</c> and <c>π/3</c> as decimals, for value comparisons where a trailing zero would be normalised away.</summary>
	private const string PiOverSix = "0.52359877559829887307710723054658381403286156656252";
	private const string PiOverThree = "1.0471975511965977461542144610931676280657231331250";

	/// <summary>The first fifty significant digits of <c>√3 / 2</c>, the cosine of <c>π/6</c>.</summary>
	private const string RootThreeOverTwoDigits = "86602540378443864676372317075293618347140262690519";

	/// <summary>
	/// The sine of one million radians, to a reference far wider than a <see cref="double"/> holds.
	/// </summary>
	/// <remarks>
	/// A hand-rolled argument reduction against a short π cannot reach these digits: a π of only the
	/// twenty-six digits the type once carried leaves about twenty-one usable digits of a
	/// <c>10^6</c> argument, so a reference this wide is a direct test of the constant behind the
	/// reduction as much as of the series in front of it.
	/// </remarks>
	private const string SinOneMillionDigits =
		"-0.3499935021712929521176524867807714690614066053287162738570590546446412263954505050656668976688940081127331690567910649695709417663";

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
	/// A spread of angles across several revolutions and both signs, including magnitudes whose
	/// reduction depends on a π far wider than the answer.
	/// </summary>
	private static PreciseNumber[] AngleSweep() =>
	[
		Parse("0.3"),
		Parse("-0.3"),
		Parse("1"),
		Parse("2.7"),
		Parse("-5.5"),
		Parse("3.14159"),
		Parse("100"),
		Parse("-1000"),
		Parse("1000000"),
		Parse("1E-40"),
	];

	[TestMethod]
	public void TestSinCosTanMatchPublishedDigits()
	{
		Assert.AreEqual(Sin1Digits, Digits(PreciseNumber.Sin(PreciseNumber.One, 50)), "Sin(1) is wrong");
		Assert.AreEqual(Cos1Digits, Digits(PreciseNumber.Cos(PreciseNumber.One, 50)), "Cos(1) is wrong");
		Assert.AreEqual(Tan1Digits, Digits(PreciseNumber.Tan(PreciseNumber.One, 50)), "Tan(1) is wrong");
	}

	[TestMethod]
	public void TestSineOfKnownAnglesFromTheirRadianConstants()
	{
		// sin(π/6) = 1/2 exactly, and cos(π/6) = √3/2, checked from the angle rather than asserted.
		Assert.AreEqual("0.5", PreciseNumber.Sin(Parse("0." + PiOverSixDigits), 48).ToString());
		Assert.AreEqual(RootThreeOverTwoDigits, Digits(PreciseNumber.Cos(Parse("0." + PiOverSixDigits), 50)));
	}

	[TestMethod]
	public void TestSinOfZeroAndCosOfZero()
	{
		Assert.AreEqual(PreciseNumber.Zero, PreciseNumber.Sin(PreciseNumber.Zero));
		Assert.AreEqual(PreciseNumber.One, PreciseNumber.Cos(PreciseNumber.Zero));
	}

	[TestMethod]
	public void TestSinReducesALargeArgumentAgainstAWidePi()
	{
		// This is the assertion that ties the trigonometry to the width of Pi. A reduction against a
		// short constant loses roughly one digit per digit of the argument, so a 10^6 argument held
		// to 120 digits could not be produced from a 26-digit Pi at all.
		PreciseNumber sine = PreciseNumber.Sin(Parse("1000000"), 130);
		AssertAgreesTo(Parse(SinOneMillionDigits), sine, 120, "Sin(1000000) did not match its wide reference");
	}

	[TestMethod]
	public void TestPythagoreanIdentityHoldsAcrossTheSweep()
	{
		foreach (PreciseNumber angle in AngleSweep())
		{
			(PreciseNumber sin, PreciseNumber cos) = PreciseNumber.SinCos(angle, 60);
			PreciseNumber identity = PreciseNumber.Add(
				PreciseNumber.Multiply(sin, sin),
				PreciseNumber.Multiply(cos, cos));
			AssertAgreesTo(PreciseNumber.One, identity, 58, $"sin² + cos² ({angle}) was not one");
		}
	}

	[TestMethod]
	public void TestSinCosSharesOneReductionWithSinAndCos()
	{
		// The member exists so a caller pays for the reduction once; both halves must equal the
		// separate calls exactly, or SinCos would be answering a different question than Sin and Cos.
		foreach (PreciseNumber angle in AngleSweep())
		{
			(PreciseNumber sin, PreciseNumber cos) = PreciseNumber.SinCos(angle, 60);
			Assert.AreEqual(PreciseNumber.Sin(angle, 60), sin, $"SinCos({angle}).Sin disagreed with Sin");
			Assert.AreEqual(PreciseNumber.Cos(angle, 60), cos, $"SinCos({angle}).Cos disagreed with Cos");
		}
	}

	[TestMethod]
	public void TestTanIsSinOverCos()
	{
		foreach (PreciseNumber angle in AngleSweep())
		{
			(PreciseNumber sin, PreciseNumber cos) = PreciseNumber.SinCos(angle, 60);
			PreciseNumber expected = PreciseNumber.Divide(sin, cos, 50);
			AssertAgreesTo(expected, PreciseNumber.Tan(angle, 50), 49, $"Tan({angle}) was not sin/cos");
		}
	}

	[TestMethod]
	public void TestSinPiIsWellDefinedAtAHugeArgument()
	{
		// SinPi(1e20) is an even multiple of π, so exactly zero; Sin(1e20 * Pi) is not defined at all,
		// which is the whole reason the half-turn family reduces on the argument before multiplying.
		Assert.AreEqual(PreciseNumber.Zero, PreciseNumber.SinPi(Parse("1E20"), 50));
		Assert.AreEqual(PreciseNumber.One, PreciseNumber.SinPi(Parse("0.5"), 50));
		Assert.AreEqual(PreciseNumber.NegativeOne, PreciseNumber.CosPi(PreciseNumber.One, 50));
		Assert.AreEqual(PreciseNumber.Zero, PreciseNumber.SinPi(Parse("2"), 50));
	}

	[TestMethod]
	public void TestSinPiAgreesWithSinOfTheAngleInRadians()
	{
		foreach (PreciseNumber halfTurns in new[] { Parse("0.1"), Parse("0.37"), Parse("-0.8"), Parse("1.25") })
		{
			PreciseNumber radians = PreciseNumber.Multiply(halfTurns, PreciseNumber.PiTo(70));
			AssertAgreesTo(PreciseNumber.Sin(radians, 55), PreciseNumber.SinPi(halfTurns, 50), 49, $"SinPi({halfTurns})");
			AssertAgreesTo(PreciseNumber.Cos(radians, 55), PreciseNumber.CosPi(halfTurns, 50), 49, $"CosPi({halfTurns})");
		}
	}

	[TestMethod]
	public void TestAtanMatchesPiOverFour() =>
		Assert.AreEqual(PiOverFourDigits, Digits(PreciseNumber.Atan(PreciseNumber.One, 50)));

	[TestMethod]
	public void TestAsinAndAcosMatchTheirKnownAngles()
	{
		Assert.AreEqual(PiOverSixDigits, Digits(PreciseNumber.Asin(Parse("0.5"), 50)), "Asin(0.5) is not π/6");
		AssertAgreesTo(Parse(PiOverSix), PreciseNumber.Asin(Parse("0.5"), 50), 49, "Asin(0.5) is not π/6");
		AssertAgreesTo(Parse(PiOverThree), PreciseNumber.Acos(Parse("0.5"), 50), 49, "Acos(0.5) is not π/3");
	}

	[TestMethod]
	public void TestAsinAndAcosAtTheEndpoints()
	{
		// The endpoints are where asin's √(1 - x²) is zero, so they are special-cased rather than
		// divided by zero.
		AssertAgreesTo(PreciseNumber.Divide(PreciseNumber.PiTo(60), Parse("2"), 55), PreciseNumber.Asin(PreciseNumber.One, 50), 49, "Asin(1)");
		Assert.AreEqual(PreciseNumber.Zero, PreciseNumber.Acos(PreciseNumber.One, 50));
		AssertAgreesTo(PreciseNumber.PiTo(60), PreciseNumber.Acos(PreciseNumber.NegativeOne, 50), 49, "Acos(-1)");
	}

	[TestMethod]
	public void TestInverseFunctionsRoundTripSineAndCosine()
	{
		foreach (PreciseNumber value in new[] { Parse("0.1"), Parse("-0.4"), Parse("0.9"), Parse("0.999") })
		{
			AssertAgreesTo(value, PreciseNumber.Sin(PreciseNumber.Asin(value, 60), 60), 49, $"Sin(Asin({value}))");
			AssertAgreesTo(value, PreciseNumber.Cos(PreciseNumber.Acos(value, 60), 60), 49, $"Cos(Acos({value}))");
			AssertAgreesTo(value, PreciseNumber.Tan(PreciseNumber.Atan(value, 60), 60), 49, $"Tan(Atan({value}))");
		}
	}

	[TestMethod]
	public void TestAtanReducesALargeArgument()
	{
		// atan of a large value approaches π/2, and the half-angle reduction is what gets it there
		// without a series that never converges.
		AssertAgreesTo(
			PreciseNumber.Divide(PreciseNumber.PiTo(60), Parse("2"), 55),
			PreciseNumber.Atan(Parse("1E30"), 50),
			30,
			"Atan(1E30) did not approach π/2");
	}

	[TestMethod]
	public void TestInverseFunctionsRejectValuesOutsideTheirDomain()
	{
		// There is no NaN to return, so the domain is enforced.
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.Asin(Parse("1.5")));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.Acos(Parse("-2")));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.Sin(PreciseNumber.One, 0));
	}

	[TestMethod]
	public void TestAtan2PlacesTheAngleInEveryQuadrant()
	{
		PreciseNumber pi = PreciseNumber.PiTo(55);
		PreciseNumber quarterPi = PreciseNumber.Divide(pi, Parse("4"), 55);
		PreciseNumber threeQuarterPi = PreciseNumber.Multiply(Parse("3"), quarterPi);

		AssertAgreesTo(quarterPi, PreciseNumber.Atan2(PreciseNumber.One, PreciseNumber.One, 50), 49, "atan2(+, +)");
		AssertAgreesTo(threeQuarterPi, PreciseNumber.Atan2(PreciseNumber.One, PreciseNumber.NegativeOne, 50), 49, "atan2(+, -)");
		AssertAgreesTo(-threeQuarterPi, PreciseNumber.Atan2(PreciseNumber.NegativeOne, PreciseNumber.NegativeOne, 50), 49, "atan2(-, -)");
		AssertAgreesTo(-quarterPi, PreciseNumber.Atan2(PreciseNumber.NegativeOne, PreciseNumber.One, 50), 49, "atan2(-, +)");
	}

	[TestMethod]
	public void TestAtan2OnTheAxes()
	{
		PreciseNumber pi = PreciseNumber.PiTo(55);
		PreciseNumber halfPi = PreciseNumber.Divide(pi, Parse("2"), 55);

		Assert.AreEqual(PreciseNumber.Zero, PreciseNumber.Atan2(PreciseNumber.Zero, PreciseNumber.Zero, 50));
		Assert.AreEqual(PreciseNumber.Zero, PreciseNumber.Atan2(PreciseNumber.Zero, PreciseNumber.One, 50));
		AssertAgreesTo(halfPi, PreciseNumber.Atan2(PreciseNumber.One, PreciseNumber.Zero, 50), 49, "atan2(+, 0)");
		AssertAgreesTo(-halfPi, PreciseNumber.Atan2(PreciseNumber.NegativeOne, PreciseNumber.Zero, 50), 49, "atan2(-, 0)");
		AssertAgreesTo(pi, PreciseNumber.Atan2(PreciseNumber.Zero, PreciseNumber.NegativeOne, 50), 49, "atan2(0, -)");
	}

	[TestMethod]
	public void TestAgreesWithDoublePrecisionAsACheapRegressionNet()
	{
		foreach (double sample in new[] { 0.3, 2.7, -5.5, 1.0, 0.75 })
		{
			PreciseNumber argument = Parse(sample.ToString("R", CultureInfo.InvariantCulture));
			AssertAgreesTo(Parse(Math.Sin(sample).ToString("R", CultureInfo.InvariantCulture)), PreciseNumber.Sin(argument, 20), 14, $"Sin({sample})");
			AssertAgreesTo(Parse(Math.Cos(sample).ToString("R", CultureInfo.InvariantCulture)), PreciseNumber.Cos(argument, 20), 14, $"Cos({sample})");
			AssertAgreesTo(Parse(Math.Atan(sample).ToString("R", CultureInfo.InvariantCulture)), PreciseNumber.Atan(argument, 20), 14, $"Atan({sample})");
		}

		AssertAgreesTo(
			Parse(Math.Atan2(1.0, -1.0).ToString("R", CultureInfo.InvariantCulture)),
			PreciseNumber.Atan2(PreciseNumber.One, PreciseNumber.NegativeOne, 20),
			14,
			"Atan2(1, -1)");
	}

	[TestMethod]
	public void TestDegreeAndRadianConversionUseACorrectlyRoundedPi()
	{
		AssertAgreesTo(PreciseNumber.PiTo(55), PreciseNumber.DegreesToRadians(Parse("180"), 50), 49, "180° in radians is π");
		Assert.AreEqual("180", PreciseNumber.RadiansToDegrees(PreciseNumber.PiTo(55), 50).ToString());
		AssertAgreesTo(Parse("45"), PreciseNumber.RadiansToDegrees(PreciseNumber.Divide(PreciseNumber.PiTo(55), Parse("4"), 55), 50), 48, "π/4 in degrees is 45");
	}

	[TestMethod]
	public void TestInverseHalfTurnFamilyReturnsHalfTurns()
	{
		// asin/acos/atan divided by π: the endpoints are exact.
		Assert.AreEqual(Parse("0.5"), PreciseNumber.AsinPi(PreciseNumber.One, 50));
		Assert.AreEqual(PreciseNumber.Zero, PreciseNumber.AcosPi(PreciseNumber.One, 50));
		Assert.AreEqual(PreciseNumber.One, PreciseNumber.AcosPi(PreciseNumber.NegativeOne, 50));
		AssertAgreesTo(Parse("0.25"), PreciseNumber.AtanPi(PreciseNumber.One, 50), 49, "AtanPi(1) is a quarter turn");
	}
}
